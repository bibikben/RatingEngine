namespace BTSS.Rating.Application.Abstractions;

public interface IContractVersionPublisher
{
    Task PublishAsync(long contractVersionId, string? userId = null, string? note = null, CancellationToken ct = default);
}
