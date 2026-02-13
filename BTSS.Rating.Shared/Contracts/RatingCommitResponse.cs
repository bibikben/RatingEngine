namespace BTSS.Rating.Shared.Contracts;

public sealed record RatingCommitResponse(
    string RateQuoteId,
    string RateQuoteResultId,
    RatingQuoteResponse Quote
);
