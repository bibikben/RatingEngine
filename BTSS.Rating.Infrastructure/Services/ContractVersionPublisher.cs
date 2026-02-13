using BTSS.Rating.Application.Abstractions;
using BTSS.Rating.Infrastructure.Persistence;
using BTSS.Rating.Infrastructure.Persistence.Entities;
using BTSS.Rating.Persistence;
using Microsoft.EntityFrameworkCore;
using ContractStatusHistory = BTSS.Rating.Models.ContractStatusHistory;

namespace BTSS.Rating.Infrastructure.Services;

public sealed class ContractVersionPublisher : IContractVersionPublisher
{
    private readonly RatingDbContext _db;

    public ContractVersionPublisher(RatingDbContext db) => _db = db;

    public async Task PublishAsync(long contractVersionId, string? userId, string? note, CancellationToken ct = default)
    {
        var version = await _db.ContractVersions.FirstOrDefaultAsync(v => v.ContractVersionId == contractVersionId, ct)
            ?? throw new InvalidOperationException($"ContractVersion {contractVersionId} not found.");

        // Basic validation: must have at least one lane eligibility row OR at least one rate row for the mode.
        // (Extend this later.)
        if (version.PublishStatus == "Published")
            return;

        version.PublishStatus = "Published";
        version.PublishedAt = DateTime.UtcNow;

        var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.ContractId == version.ContractId, ct)
            ?? throw new InvalidOperationException($"Contract {version.ContractId} not found.");

        contract.Status = "Published";

        _db.ContractStatusHistories.Add(new ContractStatusHistory
        {
            ContractId = contract.ContractId,
            ContractVersionId = version.ContractVersionId,
            FromStatus = contract.Status, // note: status already set above; keep simple in MVP
            ToStatus = "Published",
            ChangedAt = DateTime.UtcNow,
            UserId = userId,
            Note = note
        });

        await _db.SaveChangesAsync(ct);
    }
}