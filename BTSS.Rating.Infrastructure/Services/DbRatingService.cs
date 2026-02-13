using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Infrastructure.Persistence.Entities;
using BTSS.Rating.Shared.Contracts;
using BTSS.Rating.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class DbRatingService : IRatingService
{
    private readonly RatingDbContext _db;

    public DbRatingService(RatingDbContext db)
    {
        _db = db;
    }

    public async Task<RatingQuoteResponse> QuoteAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        var warnings = new List<string>();

        // Resolve correlation id
        Guid requestId = Guid.TryParse(request.RequestId, out var rid) ? rid : Guid.NewGuid();

        // Basic input validation (additional validation handled in API model validation as well)
        if (request.Lines is null || request.Lines.Count == 0)
            return new RatingQuoteResponse(requestId.ToString("N"), 0m, Array.Empty<RatingChargeLine>(), new[] { "No shipment lines were provided." });

        // Resolve origin/dest zones (for LTL/lane eligibility)
        var originZone = await ResolveZoneIdAsync(request.Origin.PostalCode, ct);
        var destZone = await ResolveZoneIdAsync(request.Destination.PostalCode, ct);

        if (originZone is null || destZone is null)
            warnings.Add("Unable to resolve origin/destination zones from PostalCode; zone-based tables may not match.");

        // Resolve account/contract/version/provider/currency
        var ctx = await ResolveContractContextAsync(request, ct, warnings);

        // Lane eligibility check (if we have zones)
        if (ctx.ContractVersionId is not null && originZone is not null && destZone is not null)
        {
            var laneOk = await _db.ContractLaneEligibilities.AsNoTracking()
                .Where(l => l.ContractVersionId == ctx.ContractVersionId.Value)
                .AnyAsync(l =>
                    (l.OriginZoneId == null || l.OriginZoneId == originZone) &&
                    (l.DestZoneId == null || l.DestZoneId == destZone) &&
                    (l.OriginRegionId == null || l.OriginRegionId == null) && // region matching optional; omitted here
                    (l.DestRegionId == null || l.DestRegionId == null) &&
                    (l.Mode == null || l.Mode == request.Mode.ToString()),
                    ct);

            if (!laneOk)
                warnings.Add("No ContractLaneEligibility match found for origin/destination; rating may be incomplete.");
        }

        // Build charges
        var charges = new List<RatingChargeLine>();

        decimal linehaul = request.Mode switch
        {
            ShipmentMode.LTL => await RateLtlAsync(request, ctx, originZone, destZone, charges, warnings, ct),
            ShipmentMode.FTL => await RateFtlAsync(request, ctx, charges, warnings, ct),
            ShipmentMode.FCL => await RateFclAsync(request, ctx, charges, warnings, ct),
            ShipmentMode.LCL => await RateLclAsync(request, ctx, charges, warnings, ct),
            _ => 0m
        };

        // Accessorials
        var accessorialTotal = await ApplyAccessorialsAsync(request, ctx, linehaul, charges, warnings, ct);

        // Fuel
        var fuelTotal = await ApplyFuelAsync(request, ctx, linehaul, linehaul + accessorialTotal, charges, warnings, ct);

        var total = charges.Sum(c => c.Amount);

        return new RatingQuoteResponse(
            QuoteId: requestId.ToString("N"),
            Total: total,
            Charges: charges,
            Warnings: warnings
        );
    }

    private async Task<int?> ResolveZoneIdAsync(string? postalCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(postalCode)) return null;
        var zip = postalCode.Trim();
        var row = await _db.GeoZipZones.AsNoTracking()
            .Where(z => z.Zip == zip)
            .Select(z => new { z.ZoneId })
            .FirstOrDefaultAsync(ct);
        return row?.ZoneId;
    }

    private sealed record ContractContext(
        long? AccountId,
        long? ContractId,
        long? ContractVersionId,
        long? ProviderId,
        string CurrencyCode
    );

    private async Task<ContractContext> ResolveContractContextAsync(RatingQuoteRequest request, CancellationToken ct, List<string> warnings)
    {
        long? accountId = null;
        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var acct = await _db.Accounts.AsNoTracking()
                .Where(a => a.AccountCode == request.CustomerId)
                .Select(a => new { a.AccountId })
                .FirstOrDefaultAsync(ct);

            accountId = acct?.AccountId;
            if (accountId is null)
                warnings.Add($"No Account found for CustomerId/AccountCode '{request.CustomerId}'.");
        }

        long? contractId = null;
        if (!string.IsNullOrWhiteSpace(request.ContractId) && long.TryParse(request.ContractId, out var cid))
            contractId = cid;

        // Contract selection
        Contract? contract = null;
        if (contractId is not null)
        {
            contract = await _db.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ContractId == contractId.Value, ct);
        }
        else if (accountId is not null)
        {
            contract = await _db.Contracts.AsNoTracking()
                .Where(c => c.AccountId == accountId.Value && c.IsActive && c.Mode == request.Mode.ToString())
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        if (contract is null)
        {
            warnings.Add("No contract was resolved; rates may not match contract-specific tables.");
            return new ContractContext(accountId, null, null, null, "USD");
        }

        contractId = contract.ContractId;

        // Published version effective on ship date
        var shipDate = request.ShipDate;
        var version = await _db.ContractVersions.AsNoTracking()
            .Where(v => v.ContractId == contractId.Value &&
                        v.PublishStatus == "Published" &&
                        v.EffectiveFrom <= shipDate &&
                        v.EffectiveTo >= shipDate)
            .OrderByDescending(v => v.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        if (version is null)
        {
            warnings.Add("No published ContractVersion effective for the ship date.");
            return new ContractContext(accountId, contractId, null, contract.ProviderId, contract.CurrencyCode ?? "USD");
        }

        return new ContractContext(accountId, contractId, version.ContractVersionId, contract.ProviderId, contract.CurrencyCode ?? "USD");
    }

    private async Task<decimal> RateLtlAsync(
        RatingQuoteRequest request,
        ContractContext ctx,
        int? originZone,
        int? destZone,
        List<RatingChargeLine> charges,
        List<string> warnings,
        CancellationToken ct)
    {
        if (ctx.ContractVersionId is null || originZone is null || destZone is null)
        {
            warnings.Add("LTL requires ContractVersionId and zone resolution; no LTL linehaul calculated.");
            return 0m;
        }

        decimal totalLinehaul = 0m;

        foreach (var line in request.Lines)
        {
            if (!int.TryParse(line.FreightClass, out var nmfcClass))
            {
                warnings.Add("Missing or invalid FreightClass for an LTL line; skipping line.");
                continue;
            }

            var weightLbs = (int)Math.Ceiling(line.Weight);

            var baseRow = await _db.LtlBaseRates.AsNoTracking()
                .Where(r => r.ContractVersionId == ctx.ContractVersionId.Value
                            && r.OriginZoneId == originZone.Value
                            && r.DestZoneId == destZone.Value
                            && r.NmfcClass == nmfcClass
                            && r.WeightMinLbs <= weightLbs
                            && r.WeightMaxLbs >= weightLbs
                            && r.EffectiveDate <= request.ShipDate
                            && r.ExpirationDate >= request.ShipDate)
                .OrderByDescending(r => r.WeightMinLbs)
                .FirstOrDefaultAsync(ct);

            if (baseRow is null)
            {
                warnings.Add($"No LTL base rate found for class {nmfcClass}, weight {weightLbs} lbs, zones {originZone}->{destZone}.");
                continue;
            }

            var cwt = line.Weight / 100m;
            var baseAmount = cwt * baseRow.RatePerCwt;

            // Apply discount rule (specific class first, then wildcard)
            var disc = await _db.LtlDiscountRules.AsNoTracking()
                .Where(d => d.ContractVersionId == ctx.ContractVersionId.Value
                            && d.EffectiveDate <= request.ShipDate
                            && d.ExpirationDate >= request.ShipDate
                            && (d.NmfcClass == nmfcClass || d.NmfcClass == null))
                .OrderByDescending(d => d.NmfcClass.HasValue) // prefer exact class
                .ThenByDescending(d => d.DiscountPercent)
                .FirstOrDefaultAsync(ct);

            var discountPct = disc?.DiscountPercent ?? 0m;
            var discounted = baseAmount * (1m - (discountPct / 100m));

            // Minimum charge logic
            decimal? minCharge = disc?.MinChargeOverride ?? baseRow.MinimumCharge;
            if (minCharge.HasValue && discounted < minCharge.Value)
            {
                discounted = minCharge.Value;
                warnings.Add($"Minimum charge applied for class {nmfcClass}.");
            }

            if (!string.IsNullOrWhiteSpace(baseRow.DeficitRuleJson))
                warnings.Add("DeficitRuleJson present for LTL base rate but not applied (MVP).");

            totalLinehaul += discounted;
        }

        if (totalLinehaul > 0)
            charges.Add(new RatingChargeLine("LINEHAUL", "LTL linehaul", totalLinehaul));

        return totalLinehaul;
    }

    private async Task<decimal> RateFtlAsync(
        RatingQuoteRequest request,
        ContractContext ctx,
        List<RatingChargeLine> charges,
        List<string> warnings,
        CancellationToken ct)
    {
        if (ctx.ContractVersionId is null)
        {
            warnings.Add("FTL requires ContractVersionId; no FTL linehaul calculated.");
            return 0m;
        }

        // FTL table uses Location/Region or free-form? We'll match by origin/dest region if possible.
        // MVP: match by EquipmentType (nullable) and shipdate window.
        var eq = request.EquipmentType;

        var rate = await _db.FtlLaneRates.AsNoTracking()
            .Where(r => r.ContractVersionId == ctx.ContractVersionId.Value
                        && r.EffectiveDate <= request.ShipDate
                        && r.ExpirationDate >= request.ShipDate
                        && (eq == null || r.EquipmentType == null || r.EquipmentType == eq))
            .OrderByDescending(r => r.EquipmentType == eq) // prefer exact match
            .FirstOrDefaultAsync(ct);

        if (rate is null)
        {
            warnings.Add("No FTL lane rate found for the contract/version/date.");
            return 0m;
        }

        var amt = rate.FlatAmount;
        charges.Add(new RatingChargeLine("LINEHAUL", "FTL linehaul", amt));
        return amt;
    }

    private async Task<decimal> RateFclAsync(
        RatingQuoteRequest request,
        ContractContext ctx,
        List<RatingChargeLine> charges,
        List<string> warnings,
        CancellationToken ct)
    {
        if (ctx.ContractVersionId is null)
        {
            warnings.Add("FCL requires ContractVersionId; no FCL linehaul calculated.");
            return 0m;
        }

        if (string.IsNullOrWhiteSpace(request.OriginPort) || string.IsNullOrWhiteSpace(request.DestinationPort))
        {
            warnings.Add("OriginPort/DestinationPort are required for FCL rating.");
            return 0m;
        }

        var container = request.ContainerType;

        var row = await _db.FclContainerRates.AsNoTracking()
            .Where(r => r.ContractVersionId == ctx.ContractVersionId.Value
                        && r.OriginPort == request.OriginPort
                        && r.DestPort == request.DestinationPort
                        && r.EffectiveDate <= request.ShipDate
                        && r.ExpirationDate >= request.ShipDate
                        && (container == null || r.ContainerType == null || r.ContainerType == container))
            .OrderByDescending(r => r.ContainerType == container)
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            warnings.Add("No FCL container rate found for the requested ports/container.");
            return 0m;
        }

        var amt = row.FlatAmount;
        charges.Add(new RatingChargeLine("LINEHAUL", "FCL linehaul", amt));
        return amt;
    }

    private async Task<decimal> RateLclAsync(
        RatingQuoteRequest request,
        ContractContext ctx,
        List<RatingChargeLine> charges,
        List<string> warnings,
        CancellationToken ct)
    {
        if (ctx.ContractVersionId is null)
        {
            warnings.Add("LCL requires ContractVersionId; no LCL linehaul calculated.");
            return 0m;
        }

        if (string.IsNullOrWhiteSpace(request.OriginPort) || string.IsNullOrWhiteSpace(request.DestinationPort))
        {
            warnings.Add("OriginPort/DestinationPort are required for LCL rating.");
            return 0m;
        }

        var totalWeight = request.Lines.Sum(l => l.Weight);
        var row = await _db.LclRates.AsNoTracking()
            .Where(r => r.ContractVersionId == ctx.ContractVersionId.Value
                        && r.OriginPort == request.OriginPort
                        && r.DestPort == request.DestinationPort
                        && r.EffectiveDate <= request.ShipDate
                        && r.ExpirationDate >= request.ShipDate
                        && r.WeightMinLbs <= totalWeight
                        && r.WeightMaxLbs >= totalWeight)
            .OrderByDescending(r => r.WeightMinLbs)
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            warnings.Add("No LCL rate found for requested ports/weight.");
            return 0m;
        }

        var amt = (totalWeight / 100m) * row.RatePerCwt;
        if (row.MinimumCharge.HasValue && amt < row.MinimumCharge.Value)
            amt = row.MinimumCharge.Value;

        charges.Add(new RatingChargeLine("LINEHAUL", "LCL linehaul", amt));
        return amt;
    }

    private async Task<decimal> ApplyAccessorialsAsync(
        RatingQuoteRequest request,
        ContractContext ctx,
        decimal linehaul,
        List<RatingChargeLine> charges,
        List<string> warnings,
        CancellationToken ct)
    {
        if (ctx.ContractVersionId is null) return 0m;
        if (request.AccessorialCodes is null || request.AccessorialCodes.Count == 0) return 0m;

        var codes = request.AccessorialCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct().ToList();
        if (codes.Count == 0) return 0m;

        var accessorials = await _db.Accessorials.AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .ToListAsync(ct);

        var byCode = accessorials.ToDictionary(a => a.Code, StringComparer.OrdinalIgnoreCase);
        decimal total = 0m;

        foreach (var code in codes)
        {
            if (!byCode.TryGetValue(code, out var acc))
            {
                warnings.Add($"Unknown accessorial code '{code}'.");
                continue;
            }

            var chargeRow = await _db.ContractAccessorialCharges.AsNoTracking()
                .Where(c => c.ContractVersionId == ctx.ContractVersionId.Value
                            && c.AccessorialId == acc.AccessorialId
                            && c.EffectiveDate <= request.ShipDate
                            && c.ExpirationDate >= request.ShipDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync(ct);

            if (chargeRow is null)
            {
                warnings.Add($"No contract accessorial charge configured for '{code}'.");
                continue;
            }

            decimal amount = 0m;
            var calcType = (chargeRow.CalcType ?? "").Trim();

            if (calcType.Equals("Flat", StringComparison.OrdinalIgnoreCase))
            {
                amount = chargeRow.FlatAmount ?? 0m;
            }
            else if (calcType.Equals("Percent", StringComparison.OrdinalIgnoreCase))
            {
                var pct = chargeRow.PercentValue ?? 0m;
                var baseAmt = chargeRow.ApplyTo?.Equals("TOTAL", StringComparison.OrdinalIgnoreCase) == true ? linehaul : linehaul;
                amount = baseAmt * (pct / 100m);
            }
            else
            {
                // MVP: treat unknown as flat if provided
                amount = chargeRow.FlatAmount ?? 0m;
                warnings.Add($"Accessorial '{code}' CalcType '{calcType}' not fully supported (MVP).");
            }

            if (chargeRow.MinAmount.HasValue && amount < chargeRow.MinAmount.Value)
                amount = chargeRow.MinAmount.Value;
            if (chargeRow.MaxAmount.HasValue && amount > chargeRow.MaxAmount.Value)
                amount = chargeRow.MaxAmount.Value;

            if (amount != 0m)
            {
                var desc = acc.Description ?? "Accessorial";
                charges.Add(new RatingChargeLine(code.ToUpperInvariant(), desc, amount));
                total += amount;
            }
        }

        return total;
    }

    private async Task<decimal> ApplyFuelAsync(
        RatingQuoteRequest request,
        ContractContext ctx,
        decimal linehaul,
        decimal totalBeforeFuel,
        List<RatingChargeLine> charges,
        List<string> warnings,
        CancellationToken ct)
    {
        if (ctx.ContractVersionId is null) return 0m;

        var fuelRule = await _db.ContractFuelRules.AsNoTracking()
            .Where(r => r.ContractVersionId == ctx.ContractVersionId.Value
                        && r.EffectiveDate <= request.ShipDate
                        && r.ExpirationDate >= request.ShipDate)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(ct);

        if (fuelRule is null) return 0m;

        var scheduleRow = await _db.FuelScheduleRows.AsNoTracking()
            .Where(r => r.FuelScheduleId == fuelRule.FuelScheduleId
                        && r.EffectiveStart <= request.ShipDate
                        && r.EffectiveEnd >= request.ShipDate)
            .OrderByDescending(r => r.EffectiveStart)
            .FirstOrDefaultAsync(ct);

        if (scheduleRow is null)
        {
            warnings.Add("Fuel rule found but no schedule row matches ship date.");
            return 0m;
        }

        // MVP: FuelValue is treated as a percent when CalcMethod == Percent
        var method = (fuelRule.CalcMethod ?? "").Trim();
        decimal fuelAmount;

        if (method.Equals("Percent", StringComparison.OrdinalIgnoreCase))
        {
            var pct = scheduleRow.FuelValue;
            var applyTo = (fuelRule.ApplyTo ?? "").Trim().ToUpperInvariant();
            var baseAmt = applyTo == "TOTAL" ? totalBeforeFuel : linehaul;
            fuelAmount = baseAmt * (pct / 100m);
        }
        else if (method.Equals("Flat", StringComparison.OrdinalIgnoreCase))
        {
            fuelAmount = scheduleRow.FuelValue;
        }
        else
        {
            // fallback: percent
            var pct = scheduleRow.FuelValue;
            fuelAmount = linehaul * (pct / 100m);
            warnings.Add($"Fuel CalcMethod '{method}' not fully supported; treated as Percent (MVP).");
        }

        if (fuelAmount != 0m)
        {
            charges.Add(new RatingChargeLine("FUEL", "Fuel surcharge", fuelAmount));
        }

        return fuelAmount;
    }
}