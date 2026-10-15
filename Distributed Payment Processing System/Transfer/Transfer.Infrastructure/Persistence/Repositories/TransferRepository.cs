using Transfer.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
using Transfer.Infrastructure.Common;
using Transfer.Application.Abstractions.Persistence;
using Npgsql;
namespace Transfer.Infrastructure.Persistence.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly TransferDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    public TransferRepository(TransferDbContext dbContext,TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<AddTransferResult> CreateOrGetAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbContext.Transfers.Add(transfer);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation &&
            pg.ConstraintName == "UX_Transfers_Sender_IdempotencyKey")
        {
            _dbContext.Entry(transfer).State = EntityState.Detached;

            var existing = await GetByIdempotencyKeyAsync(
                transfer.SenderAccountId,
                transfer.IdempotencyKey,
                cancellationToken);
            
            if(existing == null) throw new InfrastructureException("Конфликт идемпотентности, но перевод не найден", ex);

            return new AddTransferResult(existing,false);
        } 

        
        
        return new AddTransferResult(transfer, true);
    }

    public async Task<bool> AnyAsync(TransferId transferId, CancellationToken cancellationToken = default)
    {
        bool any = await _dbContext.Transfers.AnyAsync(t => t.Id == transferId, cancellationToken);
        return any;
    }

    public async Task<Domain.Transfers.Transfer?> GetByIdAsync(TransferId transferId, CancellationToken cancellationToken = default)
    {
        Domain.Transfers.Transfer? transfer = await _dbContext.Transfers.FindAsync(new object[] {transferId},cancellationToken);
        return transfer;
    }

    public async Task<Domain.Transfers.Transfer?> GetByIdempotencyKeyAsync(AccountId senderAccountId, IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
    {
        Domain.Transfers.Transfer? transfer = await _dbContext.Transfers.FirstOrDefaultAsync(t =>
        t.SenderAccountId == senderAccountId &&
        t.IdempotencyKey == idempotencyKey,cancellationToken);
        return transfer;
    }

    public async Task UpdateAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default)
    {




        transfer.UpdateVersion(_timeProvider.GetUtcNow());

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateConcurrencyException exeption)
        {
            throw new InfrastructureException("Перевод уже измененен другим процессом",exeption);
        }
        

        




    }
}

