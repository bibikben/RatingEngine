using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Infrastructure.Persistence.Entities;
using BTSS.Rating.Persistence;
using BTSS.Rating.Shared.Contracts;
using BTSS.Rating.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class DbRatingService : IRatingService
{
    private readonly RatingDbContext _db;

    public DbRatingService(RatingDbContext db) => _db = db;

    public async Task<RatingQuoteResponse> QuoteAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        var calc = await ComputeAsync(request, ct);
        return calc.Response;
    }

    internal sealed record Computation(
        RatingQuoteResponse Response,
        long AccountId,
        long ProviderId,
        long ContractId,
        long ContractVersionId
    );

    internal async Task<Computation> ComputeAsync(RatingQuoteRequest request, CancellationToken ct)
    {
        var warnings = new List<string>();
        var requestId = request.RequestId ?? Guid.NewGuid();

        // 1) Resolve Account
        var accountCode = request.CustomerId ?? request.ContractId ?? "UNKNOWN";
        var account = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountCode == accountCode, ct);
        if (account is null)
            throw new InvalidOperationException($"Account not found for code '{accountCode}'.");

        // 2) Resolve active Contract
        var modeStr = request.Mode.ToString();
        var contract = await _db.Contracts.AsNoTracking()
            .Where(c => c.AccountId == account.AccountId && c.Mode == modeStr && c.IsActive)
            .OrderByDescending(c => c.ContractId)
            .FirstOrDefaultAsync(ct);

        if (contract is null)
            throw new InvalidOperationException($"No active contract found for account '{accountCode}' and mode '{modeStr}'.");

        // 3) Resolve published ContractVersion effective on ship date
        var shipDate = request.ShipDate;
        var version = await _db.ContractVersions.AsNoTracking()
            .Where(v => v.ContractId == contract.ContractId
                        && v.PublishStatus == "Published"
                        && v.EffectiveStart <= shipDate
                        && v.EffectiveEnd >= shipDate)
            .OrderByDescending(v => v.VersionNo)
            .FirstOrDefaultAsync(ct);

        if (version is null)
            throw new InvalidOperationException($"No published contract version effective on {shipDate:yyyy-MM-dd} for contract {contract.ContractId}.");

        // 4) Geo resolution (zone/region)
        var originGeo = await ResolveGeoAsync(request.Origin, ct);
        var destGeo = await ResolveGeoAsync(request.Destination, ct);

        if (originGeo.ZoneId is null || destGeo.ZoneId is null)
            warnings.Add("Origin/Destination zone could not be resolved (GeoZipZone missing).");
        if (originGeo.RegionId is null || destGeo.RegionId is null)
            warnings.Add("Origin/Destination region could not be resolved (GeoZipZone missing).");

        // 5) Lane eligibility (best-effort)
        var laneOk = await LaneEligibleAsync(version.ContractVersionId, modeStr, originGeo, destGeo, ct);
        if (!laneOk)
            warnings.Add("No ContractLaneEligibility match found for origin/destination. Rating may be incomplete.");

        // 6) Compute charges by mode
        var charges = new List<RatingChargeLine>();
        decimal linehaul = 0m;

        switch (request.Mode)
        {
            case ShipmentMode.LTL:
                linehaul = await RateLtlAsync(version.ContractVersionId, shipDate, originGeo, destGeo, request, charges, warnings, ct);
                break;

            case ShipmentMode.FTL:
                linehaul = await RateFtlAsync(version.ContractVersionId, shipDate, originGeo, destGeo, request, charges, warnings, ct);
                break;

            case ShipmentMode.FCL:
                linehaul = await RateFclAsync(version.ContractVersionId, shipDate, request, charges, warnings, ct);
                break;

            case ShipmentMode.LCL:
                linehaul = await RateLclAsync(version.ContractVersionId, shipDate, request, charges, warnings, ct);
                break;

            default:
                warnings.Add($"Unsupported mode: {request.Mode}");
                break;
        }

        // 7) Accessorials (flat + percent supported)
        decimal subtotal = charges.Sum(c => c.Amount);
        subtotal += await ApplyAccessorialsAsync(version.ContractVersionId, shipDate, request, linehaul, subtotal, charges, warnings, ct);

        // 8) Fuel (percent rows supported)
        subtotal += await ApplyFuelAsync(version.ContractVersionId, shipDate, linehaul, subtotal, charges, warnings, ct);

        var resp = new RatingQuoteResponse(
            QuoteId: requestId.ToString("N"),
            Total: Math.Round(subtotal, 6),
            Charges: charges,
            Warnings: warnings
        );

        return new Computation(resp, account.AccountId, contract.ProviderId, contract.ContractId, version.ContractVersionId);
    }

    private async Task<(int? ZoneId, int? RegionId)> ResolveGeoAsync(Address addr, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(addr.PostalCode))
            return (null, null);

        var geo = await _db.GeoZipZones.AsNoTracking()
            .Where(g => g.CountryCode == addr.Country[..Math.Min(2, addr.Country.Length)] && g.PostalCode == addr.PostalCode)
            .OrderByDescending(g => g.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        return geo is null ? (null, null) : (geo.ZoneId, geo.RegionId);
    }

    private async Task<bool> LaneEligibleAsync(long contractVersionId, string mode, (int? ZoneId, int? RegionId) o, (int? ZoneId, int? RegionId) d, CancellationToken ct)
    {
        // Nulls in eligibility columns are treated as wildcards.
        var q = _db.ContractLaneEligibilities.AsNoTracking()
            .Where(x => x.ContractVersionId == contractVersionId && x.Mode == mode);

        if (o.ZoneId is not null) q = q.Where(x => x.OriginZoneId == null || x.OriginZoneId == o.ZoneId);
        if (d.ZoneId is not null) q = q.Where(x => x.DestZoneId == null || x.DestZoneId == d.ZoneId);
        if (o.RegionId is not null) q = q.Where(x => x.OriginRegionId == null || x.OriginRegionId == o.RegionId);
        if (d.RegionId is not null) q = q.Where(x => x.DestRegionId == null || x.DestRegionId == d.RegionId);

        return await q.AnyAsync(ct);
    }

    private async Task<decimal> RateLtlAsync(long contractVersionId, DateOnly shipDate, (int? ZoneId, int? RegionId) o, (int? ZoneId, int? RegionId) d,
        RatingQuoteRequest request, List<RatingChargeLine> charges, List<string> warnings, CancellationToken ct)
    {
        if (o.ZoneId is null || d.ZoneId is null)
        {
            warnings.Add("LTL requires ZoneId for origin/destination to lookup base rates.");
            return 0m;
        }

        var totalWeight = request.Lines.Sum(l => l.Weight);
        var classStr = request.Lines.Select(l => l.FreightClass).FirstOrDefault(fc => !string.IsNullOrWhiteSpace(fc)) ?? "55";
        if (!int.TryParse(new string(classStr.Where(char.IsDigit).ToArray()), out var nmfcClass))
            nmfcClass = 55;

        var baseRow = await _db.LtlBaseRates.AsNoTracking()
            .Where(r => r.ContractVersionId == contractVersionId
                        && r.OriginZoneId == o.ZoneId
                        && r.DestZoneId == d.ZoneId
                        && r.NmfcClass == nmfcClass
                        && r.EffectiveDate <= shipDate
                        && r.ExpirationDate >= shipDate
                        && r.WeightMinLbs <= (int)Math.Ceiling(totalWeight)
                        && r.WeightMaxLbs >= (int)Math.Ceiling(totalWeight))
            .OrderBy(r => r.WeightMinLbs)
            .FirstOrDefaultAsync(ct);

        if (baseRow is null)
        {
            warnings.Add($"No LTL base rate found for class {nmfcClass}, weight {totalWeight:0.##}, zones {o.ZoneId}->{d.ZoneId}.");
            return 0m;
        }

        var cwt = totalWeight / 100m;
        var linehaul = Math.Round(cwt * baseRow.RatePerCwt, 6);

        // Discount (most specific: class match first, then null class)
        var discRow = await _db.LtlDiscountRules.AsNoTracking()
            .Where(r => r.ContractVersionId == contractVersionId
                        && r.EffectiveDate <= shipDate
                        && r.ExpirationDate >= shipDate
                        && (r.NmfcClass == null || r.NmfcClass == nmfcClass))
            .OrderByDescending(r => r.NmfcClass.HasValue) // class-specific first
            .ThenByDescending(r => r.LtlDiscountRuleId)
            .FirstOrDefaultAsync(ct);

        if (discRow is not null && discRow.DiscountPercent != 0m)
        {
            var discountAmt = Math.Round(linehaul * (discRow.DiscountPercent / 100m), 6);
            linehaul -= discountAmt;
            charges.Add(new RatingChargeLine("DISCOUNT", $"LTL Discount {discRow.DiscountPercent:0.####}%", -discountAmt));
        }

        charges.Add(new RatingChargeLine("LINEHAUL", "LTL Linehaul", linehaul));

        // Minimum / deficit
        var min = discRow?.MinChargeOverride ?? baseRow.MinimumCharge;
        if (min is not null && linehaul < min.Value)
        {
            var deficit = Math.Round(min.Value - linehaul, 6);
            charges.Add(new RatingChargeLine("MINIMUM", "Minimum charge adjustment", deficit));
            linehaul = min.Value;
        }

        return linehaul;
    }

    private async Task<decimal> RateFtlAsync(long contractVersionId, DateOnly shipDate, (int? ZoneId, int? RegionId) o, (int? ZoneId, int? RegionId) d,
        RatingQuoteRequest request, List<RatingChargeLine> charges, List<string> warnings, CancellationToken ct)
    {
        if (o.RegionId is null || d.RegionId is null)
        {
            warnings.Add("FTL requires RegionId for origin/destination to lookup lane rates.");
            return 0m;
        }

        var equipment = "VAN";
        var row = await _db.FtlLaneRates.AsNoTracking()
            .Where(r => r.ContractVersionId == contractVersionId
                        && r.OriginRegionId == o.RegionId
                        && r.DestRegionId == d.RegionId
                        && r.EquipmentType == equipment
                        && r.EffectiveDate <= shipDate
                        && r.ExpirationDate >= shipDate)
            .OrderByDescending(r => r.FtlLaneRateId)
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            // fallback: any equipment
            row = await _db.FtlLaneRates.AsNoTracking()
                .Where(r => r.ContractVersionId == contractVersionId
                            && r.OriginRegionId == o.RegionId
                            && r.DestRegionId == d.RegionId
                            && r.EffectiveDate <= shipDate
                            && r.ExpirationDate >= shipDate)
                .OrderByDescending(r => r.FtlLaneRateId)
                .FirstOrDefaultAsync(ct);
        }

        if (row is null)
        {
            warnings.Add($"No FTL lane rate found for regions {o.RegionId}->{d.RegionId}.");
            return 0m;
        }

        var linehaul = row.RateValue;
        charges.Add(new RatingChargeLine("LINEHAUL", "FTL Linehaul", linehaul));

        if (row.MinimumCharge is not null && linehaul < row.MinimumCharge.Value)
        {
            var deficit = Math.Round(row.MinimumCharge.Value - linehaul, 6);
            charges.Add(new RatingChargeLine("MINIMUM", "Minimum charge adjustment", deficit));
            linehaul = row.MinimumCharge.Value;
        }

        return linehaul;
    }

    private async Task<decimal> RateFclAsync(long contractVersionId, DateOnly shipDate, RatingQuoteRequest request,
        List<RatingChargeLine> charges, List<string> warnings, CancellationToken ct)
    {
        // FCL tables require ports; expect caller to provide via Address.City as a temporary workaround or extend request model later.
        var originPort = request.Origin.City ?? request.Origin.PostalCode ?? "";
        var destPort = request.Destination.City ?? request.Destination.PostalCode ?? "";
        var containerType = request.ContainerType;

        var row = await _db.FclContainerRates.AsNoTracking()
            .Where(r => r.ContractVersionId == contractVersionId
                        && r.OriginPort == originPort
                        && r.DestPort == destPort
                        && r.ContainerType == containerType
                        && r.EffectiveDate <= shipDate
                        && r.ExpirationDate >= shipDate)
            .OrderByDescending(r => r.FclContainerRateId)
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            warnings.Add("No FCL container rate matched. Provide Origin/Dest port codes to rate accurately.");
            return 0m;
        }

        charges.Add(new RatingChargeLine("LINEHAUL", "FCL Base Rate", row.BaseRate));
        return row.BaseRate;
    }

    private async Task<decimal> RateLclAsync(long contractVersionId, DateOnly shipDate, RatingQuoteRequest request,
        List<RatingChargeLine> charges, List<string> warnings, CancellationToken ct)
    {
        var originPort = request.Origin.City ?? request.Origin.PostalCode ?? "";
        var destPort = request.Destination.City ?? request.Destination.PostalCode ?? "";

        var row = await _db.LclRates.AsNoTracking()
            .Where(r => r.ContractVersionId == contractVersionId
                        && r.OriginPort == originPort
                        && r.DestPort == destPort
                        && r.EffectiveDate <= shipDate
                        && r.ExpirationDate >= shipDate)
            .OrderByDescending(r => r.LclRateId)
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            warnings.Add("No LCL rate matched. Provide Origin/Dest port codes to rate accurately.");
            return 0m;
        }

        var totalWeight = request.Lines.Sum(l => l.Weight);
        var linehaul = row.RatePerLb is not null ? totalWeight * row.RatePerLb.Value : row.MinimumCharge;

        if (linehaul < row.MinimumCharge)
        {
            charges.Add(new RatingChargeLine("MINIMUM", "Minimum charge adjustment", row.MinimumCharge - linehaul));
            linehaul = row.MinimumCharge;
        }

        charges.Add(new RatingChargeLine("LINEHAUL", "LCL Linehaul", Math.Round(linehaul, 6)));
        return Math.Round(linehaul, 6);
    }

    private async Task<decimal> ApplyAccessorialsAsync(long contractVersionId, DateOnly shipDate, RatingQuoteRequest request, decimal linehaul, decimal subtotal,
        List<RatingChargeLine> charges, List<string> warnings, CancellationToken ct)
    {
        if (request.AccessorialCodes is null || request.AccessorialCodes.Count == 0)
            return 0m;

        var codes = request.AccessorialCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct().ToArray();
        if (codes.Length == 0) return 0m;

        var accessorials = await _db.Accessorials.AsNoTracking().Where(a => codes.Contains(a.Code)).ToListAsync(ct);
        var byCode = accessorials.ToDictionary(a => a.Code, a => a);

        decimal added = 0m;
        foreach (var code in codes)
        {
            if (!byCode.TryGetValue(code, out var acc))
            {
                warnings.Add($"Unknown accessorial code '{code}'.");
                continue;
            }

            var chargeRow = await _db.ContractAccessorialCharges.AsNoTracking()
                .Where(c => c.ContractVersionId == contractVersionId
                            && c.AccessorialId == acc.AccessorialId
                            && c.EffectiveDate <= shipDate
                            && c.ExpirationDate >= shipDate)
                .OrderByDescending(c => c.ContractAccessorialChargeId)
                .FirstOrDefaultAsync(ct);

            if (chargeRow is null)
            {
                warnings.Add($"No contract accessorial charge found for '{code}'.");
                continue;
            }

            decimal amount = 0m;
            if (chargeRow.CalcType == "Flat" && chargeRow.FlatAmount is not null)
                amount = chargeRow.FlatAmount.Value;
            else if (chargeRow.CalcType == "Percent" && chargeRow.PercentValue is not null)
            {
                var basis = (chargeRow.ApplyTo ?? "Linehaul") switch
                {
                    "Total" => subtotal,
                    _ => linehaul
                };
                amount = Math.Round(basis * (chargeRow.PercentValue.Value / 100m), 6);
            }
            else
            {
                warnings.Add($"Accessorial '{code}' calc type '{chargeRow.CalcType}' not supported in MVP.");
                continue;
            }

            // Min/Max
            if (chargeRow.MinAmount is not null && amount < chargeRow.MinAmount.Value) amount = chargeRow.MinAmount.Value;
            if (chargeRow.MaxAmount is not null && amount > chargeRow.MaxAmount.Value) amount = chargeRow.MaxAmount.Value;

            charges.Add(new RatingChargeLine(code, acc.Description ?? code, Math.Round(amount, 6)));
            added += amount;
        }

        return Math.Round(added, 6);
    }

    private async Task<decimal> ApplyFuelAsync(long contractVersionId, DateOnly shipDate, decimal linehaul, decimal subtotal,
        List<RatingChargeLine> charges, List<string> warnings, CancellationToken ct)
    {
        var rule = await _db.ContractFuelRules.AsNoTracking()
            .Where(r => r.ContractVersionId == contractVersionId
                        && r.EffectiveDate <= shipDate
                        && r.ExpirationDate >= shipDate)
            .OrderByDescending(r => r.ContractFuelRuleId)
            .FirstOrDefaultAsync(ct);

        if (rule is null)
            return 0m;

        var row = await _db.FuelScheduleRows.AsNoTracking()
            .Where(r => r.FuelScheduleId == rule.FuelScheduleId
                        && r.EffectiveStart <= shipDate
                        && r.EffectiveEnd >= shipDate)
            .OrderByDescending(r => r.FuelScheduleRowId)
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            warnings.Add("Fuel rule exists but no FuelScheduleRow matched the ship date.");
            return 0m;
        }

        // MVP: treat FuelValue as percentage
        var basis = (rule.ApplyTo ?? "Linehaul") switch
        {
            "Total" => subtotal,
            _ => linehaul
        };
        var fuelAmount = Math.Round(basis * (row.FuelValue / 100m), 6);

        charges.Add(new RatingChargeLine("FUEL", "Fuel surcharge", fuelAmount));
        return fuelAmount;
    }
}