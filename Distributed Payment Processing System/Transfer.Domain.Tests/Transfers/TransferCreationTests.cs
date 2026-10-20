using Transfer.Domain.Common;
using Transfer.Domain.Transfers;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;

namespace Transfer.Domain.Tests.Transfers;

public class TransferCreationTests
{
    [Fact]
    public void Create_WithValidData_CreatesTransfer()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = new Money(100, "RUB");



        var transfer = TransferAggregate.Create(
            transferId,
            senderAccountId,
            recipientAccountId,
            idempotencyKey,
            amount,
            time

            );

        Assert.Equal(TransferStatus.Created, transfer.Status);
        Assert.Equal(idempotencyKey, transfer.IdempotencyKey);
        Assert.Equal(time, transfer.CreatedAt);
        Assert.Equal(time, transfer.UpdatedAt);
        Assert.Equal(0, transfer.Version);
        Assert.Null(transfer.FailureReason);
        Assert.Equal(transferId, transfer.Id);
        Assert.Equal(amount, transfer.Amount);
        Assert.Equal(senderAccountId, transfer.SenderAccountId);
        Assert.Equal(recipientAccountId, transfer.RecipientAccountId);
    }

    [Fact]
    public void Create_WithSameSenderAndRecipient_ThrowsDomainException()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = senderAccountId;
        Money amount = new Money(100, "RUB");


        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }

    [Fact]
    public void Create_WithEmptyTransferId_ThrowsDomainException()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = default;
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = new Money(100, "RUB");


        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }

    [Fact]
    public void Create_WithEmptySenderAccountId_ThrowsDomainException()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = default;
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = new Money(100, "RUB");

        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }
    [Fact]
    public void Create_WithEmptyRecipientAccountId_ThrowsDomainException()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = default;
        Money amount = new Money(100, "RUB");

        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }
    [Fact]
    public void Create_WithEmptyIdempotencyKey_ThrowsDomainException()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = default;
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = new Money(100, "RUB");

        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }
    [Fact]
    public void Create_WithNullAmount_ThrowsDomainException()
    {
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = null!;

        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }
    [Fact]
    public void Create_WithDefaultOccurredAt_ThrowsDomainException()
    {
        DateTimeOffset time = default;


        TransferId transferId = TransferId.New();
        IdempotencyKey idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        AccountId senderAccountId = new AccountId(Guid.NewGuid());
        AccountId recipientAccountId = new AccountId(Guid.NewGuid());
        Money amount = new Money(100, "RUB");
        
        Assert.Throws<DomainException>(() =>
            TransferAggregate.Create(
                transferId,
                senderAccountId,
                recipientAccountId,
                idempotencyKey,
                amount,
                time));
    }
}
