using Transfer.Domain.Common;
using Transfer.Domain.Transfers;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;

namespace Transfer.Domain.Tests.Transfers;

public class TransferRestoreTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Restore_WithValidData_RestoresTransfer()
    {
        var id = TransferId.New();
        var senderAccountId = new AccountId(Guid.NewGuid());
        var recipientAccountId = new AccountId(Guid.NewGuid());
        var idempotencyKey = new IdempotencyKey(Guid.NewGuid());
        var amount = new Money(100, "RUB");
        var updatedAt = CreatedAt.AddMinutes(5);

        var transfer = TransferAggregate.Restore(
            id,
            senderAccountId,
            recipientAccountId,
            idempotencyKey,
            amount,
            TransferStatus.Failed,
            CreatedAt,
            updatedAt,
            3,
            "  Ошибка  ");

        Assert.Equal(id, transfer.Id);
        Assert.Equal(senderAccountId, transfer.SenderAccountId);
        Assert.Equal(recipientAccountId, transfer.RecipientAccountId);
        Assert.Equal(idempotencyKey, transfer.IdempotencyKey);
        Assert.Equal(amount, transfer.Amount);
        Assert.Equal(TransferStatus.Failed, transfer.Status);
        Assert.Equal(CreatedAt, transfer.CreatedAt);
        Assert.Equal(updatedAt, transfer.UpdatedAt);
        Assert.Equal(3, transfer.Version);
        Assert.Equal("Ошибка", transfer.FailureReason);
    }

    [Fact]
    public void Restore_WithUpdatedAtBeforeCreatedAt_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Restore(
                status: TransferStatus.Created,
                updatedAt: CreatedAt.AddTicks(-1)));
    }

    [Fact]
    public void Restore_WithNegativeVersion_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Restore(
                status: TransferStatus.Created,
                version: -1));
    }

    [Fact]
    public void Restore_WithUnknownStatus_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Restore(status: (TransferStatus)999));
    }

    [Theory]
    [InlineData(TransferStatus.Failed)]
    [InlineData(TransferStatus.Rejected)]
    public void Restore_FailedOrRejectedWithoutReason_ThrowsDomainException(
        TransferStatus status)
    {
        Assert.Throws<DomainException>(() =>
            Restore(status: status, failureReason: null));
    }

    [Theory]
    [InlineData(TransferStatus.Created)]
    [InlineData(TransferStatus.Processing)]
    [InlineData(TransferStatus.FundsReserved)]
    [InlineData(TransferStatus.Succeeded)]
    [InlineData(TransferStatus.Cancelled)]
    public void Restore_NonFailureStatusWithReason_ThrowsDomainException(
        TransferStatus status)
    {
        Assert.Throws<DomainException>(() =>
            Restore(status: status, failureReason: "Причина"));
    }

    [Fact]
    public void Restore_WithReasonLongerThan500Characters_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Restore(
                status: TransferStatus.Failed,
                failureReason: new string('x', 501)));
    }

    private static TransferAggregate Restore(
        TransferStatus status,
        DateTimeOffset? updatedAt = null,
        long version = 0,
        string? failureReason = null)
    {
        return TransferAggregate.Restore(
            TransferId.New(),
            new AccountId(Guid.NewGuid()),
            new AccountId(Guid.NewGuid()),
            new IdempotencyKey(Guid.NewGuid()),
            new Money(100, "RUB"),
            status,
            CreatedAt,
            updatedAt ?? CreatedAt,
            version,
            failureReason);
    }
}
