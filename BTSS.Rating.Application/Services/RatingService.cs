using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Shared.Contracts;
using BTSS.Rating.Shared.Enums;

namespace BTSS.Rating.Application.Services;

/// <summary>
/// Application-level rating orchestration. Replace the placeholder logic with your actual contract/lane/break rules.
/// </summary>
public sealed class RatingService : IRatingService
{
    private static readonly string[] ValidFreightClasses =
    [
        "50","55","60","65","70","77.5","85","92.5","100","110","125","150","175","200","250","300","400","500"
    ];

    public Task<RatingQuoteResponse> QuoteAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        var warnings = new List<string>();

        // LTL validations/warnings
        if (request.Mode == ShipmentMode.LTL)
        {
            for (var i = 0; i < request.Lines.Count; i++)
            {
                var line = request.Lines[i];

                if (string.IsNullOrWhiteSpace(line.FreightClass))
                    warnings.Add($"Line {i+1}: FreightClass is required for LTL.");

                if (!string.IsNullOrWhiteSpace(line.FreightClass) && !ValidFreightClasses.Contains(line.FreightClass.Trim()))
                    warnings.Add($"Line {i+1}: FreightClass '{line.FreightClass}' is not a typical NMFC class.");

                var hasAnyDim = line.LengthIn.HasValue || line.WidthIn.HasValue || line.HeightIn.HasValue;
                var hasAllDims = line.LengthIn.HasValue && line.WidthIn.HasValue && line.HeightIn.HasValue;

                if (hasAnyDim && !hasAllDims)
                    warnings.Add($"Line {i+1}: Provide all 3 dimensions (L/W/H) or none.");

                if (hasAllDims)
                {
                    var l = line.LengthIn!.Value;
                    var w = line.WidthIn!.Value;
                    var h = line.HeightIn!.Value;
                    if (l <= 0 || w <= 0 || h <= 0)
                        warnings.Add($"Line {i+1}: Dimensions must be > 0.");

                    var cubicFeet = (l * w * h) / 1728m;
                    if (cubicFeet > 0)
                    {
                        var density = line.Weight / cubicFeet;
                        if (density < 1m || density > 50m)
                            warnings.Add($"Line {i+1}: Density ({density:0.0} lb/ft³) looks unusual. Verify dims/weight.");
                    }
                }
            }
        }

        // Placeholder pricing (weight-based) until DB pricing rules are fully implemented
        var baseAmount = request.Lines.Sum(l => l.Weight) * 0.05m;

        if (request.IsHazmat == true)
        {
            warnings.Add("Hazmat indicated: ensure hazmat accessorial/rules are configured.");
            baseAmount += 50m;
        }

        var quoteId = request.RequestId;
        if (string.IsNullOrWhiteSpace(quoteId))
            quoteId = Guid.NewGuid().ToString("N");

        var charges = new List<RatingChargeLine>
        {
            new("LINEHAUL", "Base Linehaul (placeholder)", baseAmount)
        };

        // Accessorial placeholders
        if (request.AccessorialCodes is { Count: > 0 })
        {
            foreach (var code in request.AccessorialCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim().ToUpperInvariant()))
            {
                charges.Add(new(code, $"Accessorial {code} (placeholder)", 10m));
            }
        }

        var total = charges.Sum(c => c.Amount);

        var response = new RatingQuoteResponse(
            QuoteId: quoteId,
            Total: total,
            Charges: charges,
            Warnings: warnings
        );

        return Task.FromResult(response);
    }
}
