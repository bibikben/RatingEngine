using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Infrastructure.Persistence.Entities;
using BTSS.Rating.Persistence;
using BTSS.Rating.Shared.Contracts;
using RateQuote = BTSS.Rating.Models.RateQuote;
using RateQuoteChargeLine = BTSS.Rating.Models.RateQuoteChargeLine;
using RateQuoteResult = BTSS.Rating.Models.RateQuoteResult;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class RatingCommitService : IRatingCommitService
{
    private readonly RatingDbContext _db;
    private readonly DbRatingService _rating;

    public RatingCommitService(RatingDbContext db, IRatingService ratingService)
    {
        _db = db;
        _rating = ratingService as DbRatingService
            ?? throw new InvalidOperationException("RatingCommitService requires DbRatingService to be registered as IRatingService.");
    }

    public async Task<RatingCommitResponse> CommitAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        var calc = await _rating.ComputeAsync(request, ct);

        var now = DateTime.UtcNow;

        var quote = new RateQuote
        {
            RequestId = Guid.NewGuid(),
            AccountId = calc.AccountId,
            Mode = request.Mode.ToString(),
            CurrencyCode = "USD",
            RateDate = request.ShipDate,
            CreatedAt = now
        };
        _db.RateQuotes.Add(quote);
        await _db.SaveChangesAsync(ct);

        var result = new RateQuoteResult
        {
            RateQuoteId = quote.RateQuoteId,
            ProviderId = calc.ProviderId,
            ContractId = calc.ContractId,
            ContractVersionId = calc.ContractVersionId,
            Rank = 1,
            TotalAmount = calc.Response.Total,
            TransitDays = null,
            CreatedAt = now
        };
        _db.RateQuoteResults.Add(result);
        await _db.SaveChangesAsync(ct);

        var seq = 1;
        foreach (var ch in calc.Response.Charges)
        {
            _db.RateQuoteChargeLines.Add(new RateQuoteChargeLine
            {
                RateQuoteResultId = result.RateQuoteResultId,
                SequenceNo = seq++,
                CanonicalChargeType = ch.Code,
                AccessorialCode = ch.Code,
                EdiStandard = null,
                EdiChargeCode = null,
                Description = ch.Description,
                Quantity = null,
                Rate = null,
                Amount = ch.Amount,
                ApplyTo = null,
                StopSequence = null,
                LegSequence = null,
                DetailJson = null
            });
        }

        await _db.SaveChangesAsync(ct);

        return new RatingCommitResponse(quote.RateQuoteId, result.RateQuoteResultId, calc.Response);
    }
}