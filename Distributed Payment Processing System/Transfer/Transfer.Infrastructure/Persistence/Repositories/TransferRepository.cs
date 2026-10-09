using Transfer.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
namespace Transfer.Infrastructure.Persistence.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly TransferDbContext _dbContext;
    public TransferRepository(TransferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default)
    {
        _dbContext.Transfers.Add(transfer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(TransferId transferId, CancellationToken cancellationToken = default)
    {
        bool any = await _dbContext.Transfers.AnyAsync(t => t.Id == transferId, cancellationToken);
        return any;
    }

    public async Task<Domain.Transfers.Transfer?> GetByIdAsync(TransferId transferId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Domain.Transfers.Transfer?> GetByIdempotencyKeyAsync(AccountId senderAccountId, IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

