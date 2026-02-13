using BTSS.Rating.Shared.Contracts;

namespace BTSS.Rating.Application.Abstractions;

public interface IRatingCommitService
{
    Task<RatingCommitResponse> CommitAsync(RatingQuoteRequest request, CancellationToken ct = default);
}
