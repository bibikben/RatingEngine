using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Shared.Contracts;

namespace BTSS.Rating.Application.Services;

/// <summary>
/// Application-level rating orchestration. Replace the placeholder logic with your actual contract/lane/break rules.
/// </summary>
public sealed class RatingService : IRatingService
{
    public Task<RatingQuoteResponse> QuoteAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        // TODO: Resolve contract, match lanes, apply breaks/minimums/accessorials, etc.
        var baseAmount = request.Lines.Sum(l => l.Weight) * 0.05m; // placeholder

        var response = new RatingQuoteResponse(
            QuoteId: Guid.NewGuid().ToString("N"),
            Total: baseAmount,
            Charges: new[]
            {
                new RatingChargeLine("LINEHAUL", "Base Linehaul (placeholder)", baseAmount)
            },
            Warnings: Array.Empty<string>()
        );

        return Task.FromResult(response);
    }
}
