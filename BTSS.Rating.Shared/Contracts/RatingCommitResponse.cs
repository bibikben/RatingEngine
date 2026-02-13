namespace BTSS.Rating.Shared.Contracts;

public sealed record RatingCommitResponse(
    long RateQuoteId,
    long RateQuoteResultId,
    RatingQuoteResponse Quote
);
