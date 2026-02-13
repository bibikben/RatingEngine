using BTSS.Rating.Shared.Contracts;

namespace BTSS.Rating.Application.Abstractions;

public interface IRatingService
{
    Task<RatingQuoteResponse> QuoteAsync(RatingQuoteRequest request, CancellationToken ct = default);
}
