using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Infrastructure.Persistence.Entities;
using BTSS.Rating.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class RatingCommitService : IRatingCommitService
{
    private sealed record CommitContext(long AccountId, long ProviderId, long ContractId, long ContractVersionId, string CurrencyCode);

    private async Task<CommitContext?> ResolveContextAsync(RatingQuoteRequest request, CancellationToken ct)
    {
        // CustomerId is treated as AccountCode
        long accountId = 0;
        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var acct = await _db.Accounts.AsNoTracking()
                .Where(a => a.AccountCode == request.CustomerId)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync(ct);
            accountId = acct;
        }

        long contractId = 0;
        if (!string.IsNullOrWhiteSpace(request.ContractId))
            long.TryParse(request.ContractId, out contractId);

        Contract? contract = null;
        if (contractId != 0)
        {
            contract = await _db.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.ContractId == contractId, ct);
        }
        else if (accountId != 0)
        {
            contract = await _db.Contracts.AsNoTracking()
                .Where(c => c.AccountId == accountId && c.IsActive && c.Mode == request.Mode.ToString())
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        if (contract is null) return null;

        var version = await _db.ContractVersions.AsNoTracking()
            .Where(v => v.ContractId == contract.ContractId
                        && v.PublishStatus == "Published"
                        && v.EffectiveFrom <= request.ShipDate
                        && v.EffectiveTo >= request.ShipDate)
            .OrderByDescending(v => v.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        return new CommitContext(
            AccountId: accountId,
            ProviderId: contract.ProviderId,
            ContractId: contract.ContractId,
            ContractVersionId: version?.ContractVersionId ?? 0,
            CurrencyCode: contract.CurrencyCode ?? "USD"
        );
    }
    private readonly RatingDbContext _db;
    private readonly IRatingService _rating;

    public RatingCommitService(RatingDbContext db, IRatingService rating)
    {
        _db = db;
        _rating = rating;
    }

    public async Task<RatingCommitResponse> CommitAsync(RatingQuoteRequest request, CancellationToken ct = default)
    {
        Guid requestGuid;
        if (!Guid.TryParse(request.RequestId, out requestGuid))
            requestGuid = Guid.NewGuid();
        var existing = await _db.RateQuotes.AsNoTracking()
            .Where(q => q.RequestId == requestGuid)
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (existing is not null)
        {
            var result = await _db.RateQuoteResults.AsNoTracking()
                .Where(r => r.RateQuoteId == existing.RateQuoteId)
                .OrderBy(r => r.Rank)
                .FirstOrDefaultAsync(ct);

            var charges = result is null
                ? new List<RateQuoteChargeLine>()
                : await _db.RateQuoteChargeLines.AsNoTracking()
                    .Where(c => c.RateQuoteResultId == result.RateQuoteResultId)
                    .OrderBy(c => c.SequenceNo)
                    .ToListAsync(ct);

            var quoteResponse = new RatingQuoteResponse(
                QuoteId: existing.RequestId.ToString("N"),
                Total: result?.TotalAmount ?? 0m,
                Charges: charges.Select(c => new RatingChargeLine(
                    c.CanonicalChargeType ?? "CHARGE",
                    c.Description ?? c.CanonicalChargeType ?? "Charge",
                    c.Amount)).ToList(),
                Warnings: Array.Empty<string>()
            );
            return new RatingCommitResponse(
                RateQuoteId: existing.RateQuoteId.ToString(),
                RateQuoteResultId: result?.RateQuoteResultId.ToString() ?? string.Empty,
                Quote: quoteResponse
            );
        }
        request = request with { RequestId = requestGuid.ToString() };
        var quote = await _rating.QuoteAsync(request, ct);
        var commitCtx = await ResolveContextAsync(request, ct);
        var rq = new RateQuote
        {
            RequestId = requestGuid,
            AccountId = commitCtx?.AccountId ?? 0,
            Mode = request.Mode.ToString(),
            CurrencyCode = commitCtx?.CurrencyCode ?? "USD",
            RateDate = request.ShipDate,
            CreatedAt = DateTime.UtcNow
        };
        _db.RateQuotes.Add(rq);
        await _db.SaveChangesAsync(ct);

        long contractId = commitCtx?.ContractId ?? 0;
        long contractVersionId = commitCtx?.ContractVersionId ?? 0;
        long providerId = commitCtx?.ProviderId ?? 0;
        var rr = new RateQuoteResult
        {
            RateQuoteId = rq.RateQuoteId,
            ProviderId = providerId,
            ContractId = contractId,
            ContractVersionId = contractVersionId,
            Rank = 1,
            TotalAmount = quote.Total,
            TransitDays = null,
            CreatedAt = DateTime.UtcNow
        };
        _db.RateQuoteResults.Add(rr);
        await _db.SaveChangesAsync(ct);

        var seq = 1;
        foreach (var line in quote.Charges)
        {
            _db.RateQuoteChargeLines.Add(new RateQuoteChargeLine
            {
                RateQuoteResultId = rr.RateQuoteResultId,
                SequenceNo = seq++,
                CanonicalChargeType = line.Code,
                Description = line.Description,
                Amount = line.Amount
            });
        }

        await _db.SaveChangesAsync(ct);

        return new RatingCommitResponse(
            RateQuoteId: rq.RateQuoteId.ToString(),
            RateQuoteResultId: rr.RateQuoteResultId.ToString(),
            Quote: quote with { QuoteId = requestGuid.ToString("N") }
        );
    }
}