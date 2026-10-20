using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Transfer.Domain.Transfers;
using Transfer.Infrastructure.Persistence;
using Transfer.Infrastructure.Persistence.Repositories;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;


namespace Transfer.Infrastructure.Tests.Transfer;

public class TransferRepositoryTests
{
    private static TransferDbContext CreateDbContext()
    {

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString =
            configuration.GetConnectionString("TransferTestDatabase")
            ?? throw new InvalidOperationException(
                "Connection string TransferTestDatabase was not found.");

        var options = new DbContextOptionsBuilder<TransferDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TransferDbContext(options);
    }





    [Fact]
    public async Task CreateOrGetAsync_NewTransfer_PersistsTransferAndReturnsCreatedTrue()
    {
        

        DateTimeOffset time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = new Money(100, "RUB");
        TransferAggregate transfer = TransferAggregate.Create(
            transferId,
            senderAccountId,
            recipientAccountId,
            idempotencyKey,
            amount,
            time);

        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        TransferRepository repository = new TransferRepository(dbContext,TimeProvider.System);



        var result = await repository.CreateOrGetAsync(transfer);

        dbContext.ChangeTracker.Clear();

        var existing = await repository.GetByIdAsync(transferId);





        Assert.True(result.WasCreated);
        Assert.Equal(transfer.Id, result.Transfer.Id);
        Assert.NotNull(existing);

        Assert.Equal(transfer.Id, existing.Id);
        Assert.Equal(transfer.SenderAccountId, existing.SenderAccountId);
        Assert.Equal(transfer.RecipientAccountId, existing.RecipientAccountId);
        Assert.Equal(transfer.IdempotencyKey, existing.IdempotencyKey);
        Assert.Equal(transfer.Amount, existing.Amount);
        Assert.Equal(transfer.Status, existing.Status);
        Assert.Equal(transfer.Version, existing.Version);
        Assert.Equal(transfer.CreatedAt, existing.CreatedAt);
        Assert.Equal(transfer.UpdatedAt, existing.UpdatedAt);
        Assert.Equal(transfer.FailureReason, existing.FailureReason);

        await transaction.RollbackAsync();



    }
}
