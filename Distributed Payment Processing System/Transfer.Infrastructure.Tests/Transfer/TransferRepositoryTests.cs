using Microsoft.EntityFrameworkCore;
using Npgsql;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Domain.Transfers;
using Transfer.Infrastructure.Common;
using Transfer.Infrastructure.Persistence.Repositories;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;

namespace Transfer.Infrastructure.Tests.Transfer;

public class TransferRepositoryTests : PostgresTestBase
{
    [Fact]
    public async Task CreateOrGetAsync_NewTransfer_PersistsTransferAndReturnsCreatedTrue()
    {
        var transfer = GetTransfer();
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var result = await repository.CreateOrGetAsync(transfer);
        context.ChangeTracker.Clear();
        var persistedTransfer = await repository.GetByIdAsync(transfer.Id);

        Assert.True(result.WasCreated);
        Assert.Equal(transfer.Id, result.Transfer.Id);
        Assert.NotNull(persistedTransfer);
        AssertTransferEqual(transfer, persistedTransfer);
    }

    [Fact]
    public async Task CreateOrGetAsync_ExistingSenderAndKey_ReturnsExistingTransferAndCreatedFalse()
    {
        var firstTransfer = GetTransfer();
        await SeedAsync(firstTransfer);
        var secondTransfer = GetTransfer(
            senderAccountId: firstTransfer.SenderAccountId,
            idempotencyKey: firstTransfer.IdempotencyKey,
            recipientAccountId: firstTransfer.RecipientAccountId,
            amount: firstTransfer.Amount);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var result = await repository.CreateOrGetAsync(secondTransfer);

        Assert.False(result.WasCreated);
        AssertTransferEqual(firstTransfer, result.Transfer);
        Assert.Equal(EntityState.Detached, context.Entry(secondTransfer).State);
    }

    [Fact]
    public async Task CreateOrGetAsync_ExistingSenderAndKey_DoesNotInsertDuplicate()
    {
        var firstTransfer = GetTransfer();
        await SeedAsync(firstTransfer);
        var secondTransfer = GetTransfer(
            senderAccountId: firstTransfer.SenderAccountId,
            idempotencyKey: firstTransfer.IdempotencyKey);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var result = await repository.CreateOrGetAsync(secondTransfer);
        await context.SaveChangesAsync();

        await using var verificationContext = CreateDbContext();
        Assert.False(result.WasCreated);
        Assert.Equal(1, await verificationContext.Transfers.CountAsync(t =>
            t.SenderAccountId == firstTransfer.SenderAccountId &&
            t.IdempotencyKey == firstTransfer.IdempotencyKey));
        Assert.False(await verificationContext.Transfers.AnyAsync(t => t.Id == secondTransfer.Id));
    }

    [Fact]
    public async Task CreateOrGetAsync_SameKeyDifferentSenders_CreatesBothTransfers()
    {
        var firstTransfer = GetTransfer();
        var secondTransfer = GetTransfer(idempotencyKey: firstTransfer.IdempotencyKey);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var firstResult = await repository.CreateOrGetAsync(firstTransfer);
        var secondResult = await repository.CreateOrGetAsync(secondTransfer);

        Assert.True(firstResult.WasCreated);
        Assert.True(secondResult.WasCreated);
        await using var verificationContext = CreateDbContext();
        Assert.NotNull(await verificationContext.Transfers.FindAsync(firstTransfer.Id));
        Assert.NotNull(await verificationContext.Transfers.FindAsync(secondTransfer.Id));
        Assert.Equal(2, await verificationContext.Transfers.CountAsync(t =>
            t.IdempotencyKey == firstTransfer.IdempotencyKey));
    }

    [Fact]
    public async Task CreateOrGetAsync_ConcurrentRequestsWithSameSenderAndKey_PersistsOnlyOneTransfer()
    {
        var firstTransfer = GetTransfer();
        var secondTransfer = GetTransfer(
            senderAccountId: firstTransfer.SenderAccountId,
            idempotencyKey: firstTransfer.IdempotencyKey,
            recipientAccountId: firstTransfer.RecipientAccountId,
            amount: firstTransfer.Amount);
        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();
        var firstRepository = new TransferRepository(firstContext, TimeProvider.System);
        var secondRepository = new TransferRepository(secondContext, TimeProvider.System);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<CreateOrGetTransferResult> CreateWhenReleasedAsync(
            TransferRepository repository, TransferAggregate transfer)
        {
            await start.Task;
            return await repository.CreateOrGetAsync(transfer);
        }

        var firstTask = CreateWhenReleasedAsync(firstRepository, firstTransfer);
        var secondTask = CreateWhenReleasedAsync(secondRepository, secondTransfer);
        start.SetResult();
        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.Single(results, result => result.WasCreated);
        Assert.Single(results, result => !result.WasCreated);
        Assert.Equal(results[0].Transfer.Id, results[1].Transfer.Id);
        await using var verificationContext = CreateDbContext();
        var persisted = Assert.Single(await verificationContext.Transfers.Where(t =>
            t.SenderAccountId == firstTransfer.SenderAccountId &&
            t.IdempotencyKey == firstTransfer.IdempotencyKey).ToListAsync());
        Assert.Equal(results[0].Transfer.Id, persisted.Id);
    }

    [Fact]
    public async Task CreateOrGetAsync_DuplicatePrimaryKey_ThrowsDbUpdateException()
    {
        var firstTransfer = GetTransfer();
        await SeedAsync(firstTransfer);
        var secondTransfer = GetTransfer(transferId: firstTransfer.Id);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => repository.CreateOrGetAsync(secondTransfer));

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.NotEqual("UX_Transfers_Sender_IdempotencyKey", postgresException.ConstraintName);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTransfer_ReturnsTransfer()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var result = await repository.GetByIdAsync(transfer.Id);

        Assert.NotNull(result);
        AssertTransferEqual(transfer, result);
    }

    [Fact]
    public async Task GetByIdAsync_MissingTransfer_ReturnsNull()
    {
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        Assert.Null(await repository.GetByIdAsync(TransferId.New()));
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_ExistingSenderAndKey_ReturnsTransfer()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        var result = await repository.GetByIdempotencyKeyAsync(
            transfer.SenderAccountId, transfer.IdempotencyKey);

        Assert.NotNull(result);
        AssertTransferEqual(transfer, result);
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_DifferentSender_ReturnsNull()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        Assert.Null(await repository.GetByIdempotencyKeyAsync(
            new AccountId(Guid.NewGuid()), transfer.IdempotencyKey));
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_MissingKey_ReturnsNull()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        Assert.Null(await repository.GetByIdempotencyKeyAsync(
            transfer.SenderAccountId, new IdempotencyKey(Guid.NewGuid())));
    }

    [Fact]
    public async Task AnyAsync_ExistingTransfer_ReturnsTrue()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        Assert.True(await repository.AnyAsync(transfer.Id));
    }

    [Fact]
    public async Task AnyAsync_MissingTransfer_ReturnsFalse()
    {
        await SeedAsync(GetTransfer());
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, TimeProvider.System);

        Assert.False(await repository.AnyAsync(TransferId.New()));
    }

    [Fact]
    public async Task UpdateAsync_TrackedTransfer_PersistsChanges()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        var now = CreatedAt.AddMinutes(2);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, new FixedTimeProvider(now));
        var tracked = await repository.GetByIdAsync(transfer.Id);
        Assert.NotNull(tracked);
        tracked.StartProcessing(CreatedAt.AddMinutes(1));
        tracked.Fail("Insufficient funds", now);

        await repository.UpdateAsync(tracked);

        await using var verificationContext = CreateDbContext();
        var persisted = await verificationContext.Transfers.FindAsync(transfer.Id);
        Assert.NotNull(persisted);
        AssertTransferEqual(tracked, persisted);
        Assert.Equal(TransferStatus.Failed, persisted.Status);
        Assert.Equal("Insufficient funds", persisted.FailureReason);
    }

    [Fact]
    public async Task UpdateAsync_TrackedTransfer_IncrementsVersionAndSetsUpdatedAt()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        var now = CreatedAt.AddMinutes(1);
        await using var context = CreateDbContext();
        var repository = new TransferRepository(context, new FixedTimeProvider(now));
        var tracked = await repository.GetByIdAsync(transfer.Id);
        Assert.NotNull(tracked);

        await repository.UpdateAsync(tracked);

        await using var verificationContext = CreateDbContext();
        var persisted = await verificationContext.Transfers.FindAsync(transfer.Id);
        Assert.NotNull(persisted);
        Assert.Equal(transfer.Version + 1, persisted.Version);
        Assert.Equal(now, persisted.UpdatedAt);
        Assert.Equal(transfer.CreatedAt, persisted.CreatedAt);
    }

    [Fact]
    public async Task UpdateAsync_StaleVersion_ThrowsInfrastructureException()
    {
        var (exception, _, _) = await AttemptStaleUpdateAsync();

        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    [Fact]
    public async Task UpdateAsync_StaleVersion_DoesNotOverwritePersistedChanges()
    {
        var (_, winner, persisted) = await AttemptStaleUpdateAsync();

        AssertTransferEqual(winner, persisted);
        Assert.Equal(TransferStatus.Processing, persisted.Status);
    }

    private async Task<(InfrastructureException Exception, TransferAggregate Winner, TransferAggregate Persisted)>
        AttemptStaleUpdateAsync()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        var now = CreatedAt.AddMinutes(1);
        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();
        var firstRepository = new TransferRepository(firstContext, new FixedTimeProvider(now));
        var secondRepository = new TransferRepository(secondContext, new FixedTimeProvider(now));
        var winner = await firstRepository.GetByIdAsync(transfer.Id);
        var stale = await secondRepository.GetByIdAsync(transfer.Id);
        Assert.NotNull(winner);
        Assert.NotNull(stale);
        winner.StartProcessing(now);
        await firstRepository.UpdateAsync(winner);
        stale.Cancel(now);

        var exception = await Assert.ThrowsAsync<InfrastructureException>(
            () => secondRepository.UpdateAsync(stale));

        await using var verificationContext = CreateDbContext();
        var persisted = await verificationContext.Transfers.FindAsync(transfer.Id);
        Assert.NotNull(persisted);
        return (exception, winner, persisted);
    }

    private static void AssertTransferEqual(TransferAggregate expected, TransferAggregate actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.SenderAccountId, actual.SenderAccountId);
        Assert.Equal(expected.RecipientAccountId, actual.RecipientAccountId);
        Assert.Equal(expected.IdempotencyKey, actual.IdempotencyKey);
        Assert.Equal(expected.Amount, actual.Amount);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.UpdatedAt, actual.UpdatedAt);
        Assert.Equal(expected.FailureReason, actual.FailureReason);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
