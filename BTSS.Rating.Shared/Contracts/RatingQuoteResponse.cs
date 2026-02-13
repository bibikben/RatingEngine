namespace BTSS.Rating.Shared.Contracts;

public sealed record RatingQuoteResponse(
    string QuoteId,
    decimal Total,
    IReadOnlyList<RatingChargeLine> Charges,
    IReadOnlyList<string> Warnings
);

public sealed record RatingChargeLine(
    string Code,
    string Description,
    decimal Amount
);
