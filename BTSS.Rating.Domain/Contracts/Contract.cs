namespace BTSS.Rating.Domain.Contracts;

public sealed class Contract
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required DateOnly EffectiveFrom { get; init; }
    public required DateOnly EffectiveTo { get; init; }

    public string? Name { get; init; }
}
