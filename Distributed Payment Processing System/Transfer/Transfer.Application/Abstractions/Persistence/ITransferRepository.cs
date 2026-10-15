using Transfer.Domain.Transfers;

namespace Transfer.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task<Transfer.Domain.Transfers.Transfer?> GetByIdAsync(TransferId transferId, CancellationToken cancellationToken = default);
    Task<AddTransferResult> CreateOrGetAsync(Transfer.Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transfer.Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(TransferId transferId, CancellationToken cancellationToken = default);
    Task<Transfer.Domain.Transfers.Transfer?> GetByIdempotencyKeyAsync(AccountId senderAccountId, IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);
}
