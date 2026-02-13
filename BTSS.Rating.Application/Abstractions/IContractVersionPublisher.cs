namespace BTSS.Rating.Application.Abstractions;

public interface IContractVersionPublisher
{
    Task PublishAsync(long contractVersionId, string? userId, string? note, CancellationToken ct = default);
}
