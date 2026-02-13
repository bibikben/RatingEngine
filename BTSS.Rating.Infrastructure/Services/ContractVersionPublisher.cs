using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class ContractVersionPublisher : IContractVersionPublisher
{
    private readonly RatingDbContext _db;

    public ContractVersionPublisher(RatingDbContext db)
    {
        _db = db;
    }

    public async Task PublishAsync(long contractVersionId, string? userId = null, string? note = null, CancellationToken ct = default)
    {
        var version = await _db.ContractVersions.FirstOrDefaultAsync(v => v.ContractVersionId == contractVersionId, ct);
        if (version is null)
            throw new InvalidOperationException($"ContractVersion {contractVersionId} not found.");

        // Basic validation: must have effective dates
        if (version.EffectiveStart == default || version.EffectiveEnd == default)
            throw new InvalidOperationException("ContractVersion must have EffectiveStart and EffectiveEnd before publishing.");

        version.PublishStatus = "Published";
        version.PublishedAt = DateTime.UtcNow;

        // Update parent contract status
        var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.ContractId == version.ContractId, ct);
        if (contract is not null)
        {
            contract.Status = "Published";
            contract.PublishedDate = DateTime.UtcNow;
        }

        _db.ContractStatusHistories.Add(new ContractStatusHistory
        {
            ContractId = version.ContractId,
            ContractVersionId = version.ContractVersionId,
            Status = "Published",
            ChangedAt = DateTime.UtcNow,
            ChangedBy = userId,
            Note = note
        });

        await _db.SaveChangesAsync(ct);
    }
}
