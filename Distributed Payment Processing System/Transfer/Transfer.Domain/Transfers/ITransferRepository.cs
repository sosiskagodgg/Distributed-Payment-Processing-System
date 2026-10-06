

namespace Transfer.Domain.Transfers
{
    public interface ITransferRepository
    {
        
        Task<Transfer?> GetByIdAsync(TransferId transferId, CancellationToken cancellationToken = default);
        Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);
        Task UpdateAsync(Transfer transfer, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(TransferId transferId, CancellationToken cancellationToken = default);
        Task<Transfer?> GetByIdempotencyKeyAsync(AccountId senderAccountId, IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);
    }
}
