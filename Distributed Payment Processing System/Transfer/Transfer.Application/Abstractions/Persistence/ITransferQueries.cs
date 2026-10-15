

using Transfer.Application.DTOs;
using Transfer.Domain.Transfers;

namespace Transfer.Application.Abstractions.Persistence;

public interface ITransferQueries
{
    Task<TransferDto?> GetByIdAsync(
    TransferId id,
    CancellationToken cancellationToken);
}

