using BTSS.Rating.Domain.Contracts;

namespace BTSS.Rating.Application.Abstractions;

public interface IContractLookup
{
    Task<Contract?> GetContractAsync(long contractId, CancellationToken ct = default);
}
