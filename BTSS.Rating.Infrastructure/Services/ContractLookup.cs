using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Domain.Contracts;
using BTSS.Rating.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class ContractLookup : IContractLookup
{
    private readonly RatingDbContext _db;

    public ContractLookup(RatingDbContext db) => _db = db;

    public async Task<Contract?> GetContractAsync(long contractId, CancellationToken ct = default)
    {
        var row = await _db.Contracts.AsNoTracking()
            .Where(c => c.ContractId == contractId)
            .Select(c => new Contract
            {
                Id = c.ContractId.ToString(),
                CustomerId = (c.AccountId ?? 0).ToString(),
                EffectiveFrom = DateOnly.FromDateTime(c.CreatedAt ?? DateTime.UtcNow),
                EffectiveTo = DateOnly.MaxValue,
                Name = c.Name
            })
            .FirstOrDefaultAsync(ct);

        return row;
    }
}
