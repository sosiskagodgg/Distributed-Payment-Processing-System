using Transfer.Domain.Common;
using Transfer.Domain.Transfers;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;

namespace Transfer.Domain.Tests.Transfers;

public class TransferStatusTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StartProcessing_WhenStatusIsCreated_ChangesStatus()
    {
        var transfer = CreateTransfer();
        var occurredAt = CreatedAt.AddMinutes(1);

        transfer.StartProcessing(occurredAt);

        Assert.Equal(TransferStatus.Processing, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Fact]
    public void StartProcessing_WhenStatusIsNotCreated_ThrowsDomainException()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() =>
            transfer.StartProcessing(CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void MarkFundsReserved_WhenStatusIsProcessing_ChangesStatus()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        var occurredAt = CreatedAt.AddMinutes(2);

        transfer.MarkFundsReserved(occurredAt);

        Assert.Equal(TransferStatus.FundsReserved, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Fact]
    public void MarkFundsReserved_WhenStatusIsNotProcessing_ThrowsDomainException()
    {
        var transfer = CreateTransfer();

        Assert.Throws<DomainException>(() =>
            transfer.MarkFundsReserved(CreatedAt.AddMinutes(1)));
    }

    [Fact]
    public void Succeed_WhenStatusIsFundsReserved_ChangesStatus()
    {
        var transfer = CreateFundsReservedTransfer();
        var occurredAt = CreatedAt.AddMinutes(3);

        transfer.Succeed(occurredAt);

        Assert.Equal(TransferStatus.Succeeded, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Fact]
    public void Succeed_WhenStatusIsNotFundsReserved_ThrowsDomainException()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() =>
            transfer.Succeed(CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void Fail_WhenStatusIsProcessing_ChangesStatus()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        var occurredAt = CreatedAt.AddMinutes(2);

        transfer.Fail("  Недостаточно средств  ", occurredAt);

        Assert.Equal(TransferStatus.Failed, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Equal("Недостаточно средств", transfer.FailureReason);
    }

    [Fact]
    public void Fail_WhenStatusIsFundsReserved_ChangesStatus()
    {
        var transfer = CreateFundsReservedTransfer();
        var occurredAt = CreatedAt.AddMinutes(3);

        transfer.Fail("Ошибка зачисления", occurredAt);

        Assert.Equal(TransferStatus.Failed, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Equal("Ошибка зачисления", transfer.FailureReason);
    }

    [Fact]
    public void Fail_WhenStatusIsInvalid_ThrowsDomainException()
    {
        var transfer = CreateTransfer();

        Assert.Throws<DomainException>(() =>
            transfer.Fail("Ошибка", CreatedAt.AddMinutes(1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_WithEmptyReason_ThrowsDomainException(string? reason)
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() =>
            transfer.Fail(reason!, CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void Fail_WithReasonLongerThan500Characters_ThrowsDomainException()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        var reason = new string('x', 501);

        Assert.Throws<DomainException>(() =>
            transfer.Fail(reason, CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void Reject_WhenStatusIsProcessing_ChangesStatus()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        var occurredAt = CreatedAt.AddMinutes(2);

        transfer.Reject("  Антифрод  ", occurredAt);

        Assert.Equal(TransferStatus.Rejected, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Equal("Антифрод", transfer.FailureReason);
    }

    [Fact]
    public void Reject_WhenStatusIsNotProcessing_ThrowsDomainException()
    {
        var transfer = CreateTransfer();

        Assert.Throws<DomainException>(() =>
            transfer.Reject("Антифрод", CreatedAt.AddMinutes(1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_WithEmptyReason_ThrowsDomainException(string? reason)
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() =>
            transfer.Reject(reason!, CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void Reject_WithReasonLongerThan500Characters_ThrowsDomainException()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        var reason = new string('x', 501);

        Assert.Throws<DomainException>(() =>
            transfer.Reject(reason, CreatedAt.AddMinutes(2)));
    }

    [Fact]
    public void Cancel_WhenStatusIsCreated_ChangesStatus()
    {
        var transfer = CreateTransfer();
        var occurredAt = CreatedAt.AddMinutes(1);

        transfer.Cancel(occurredAt);

        Assert.Equal(TransferStatus.Cancelled, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Fact]
    public void Cancel_WhenStatusIsProcessing_ChangesStatus()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        var occurredAt = CreatedAt.AddMinutes(2);

        transfer.Cancel(occurredAt);

        Assert.Equal(TransferStatus.Cancelled, transfer.Status);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
        Assert.Null(transfer.FailureReason);
    }

    [Fact]
    public void Cancel_WhenFundsAreReserved_ThrowsDomainException()
    {
        var transfer = CreateFundsReservedTransfer();

        Assert.Throws<DomainException>(() =>
            transfer.Cancel(CreatedAt.AddMinutes(3)));
    }

    [Fact]
    public void ChangeStatus_WithEarlierTime_ThrowsDomainException()
    {
        var transfer = CreateTransfer();

        Assert.Throws<DomainException>(() =>
            transfer.StartProcessing(CreatedAt.AddTicks(-1)));
    }

    [Fact]
    public void UpdateVersion_WithValidTime_IncrementsVersionAndUpdatesTime()
    {
        var transfer = CreateTransfer();
        var occurredAt = CreatedAt.AddMinutes(1);

        transfer.UpdateVersion(occurredAt);

        Assert.Equal(1, transfer.Version);
        Assert.Equal(occurredAt, transfer.UpdatedAt);
    }

    [Fact]
    public void UpdateVersion_WithEarlierTime_ThrowsDomainException()
    {
        var transfer = CreateTransfer();

        Assert.Throws<DomainException>(() =>
            transfer.UpdateVersion(CreatedAt.AddTicks(-1)));
    }

    private static TransferAggregate CreateTransfer()
    {
        return TransferAggregate.Create(
            TransferId.New(),
            new AccountId(Guid.NewGuid()),
            new AccountId(Guid.NewGuid()),
            new IdempotencyKey(Guid.NewGuid()),
            new Money(100, "RUB"),
            CreatedAt);
    }

    private static TransferAggregate CreateFundsReservedTransfer()
    {
        var transfer = CreateTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        transfer.MarkFundsReserved(CreatedAt.AddMinutes(2));
        return transfer;
    }
}
