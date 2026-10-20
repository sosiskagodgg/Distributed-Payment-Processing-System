using Transfer.Domain.Transfers;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;

namespace Transfer.Domain.Tests.Transfers;

public class TransferEqualityTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Equals_WithSameTransferId_ReturnsTrue()
    {
        var id = TransferId.New();
        var first = CreateTransfer(id);
        var second = CreateTransfer(id);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_WithDifferentTransferId_ReturnsFalse()
    {
        var first = CreateTransfer(TransferId.New());
        var second = CreateTransfer(TransferId.New());

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    private static TransferAggregate CreateTransfer(TransferId id)
    {
        return TransferAggregate.Create(
            id,
            new AccountId(Guid.NewGuid()),
            new AccountId(Guid.NewGuid()),
            new IdempotencyKey(Guid.NewGuid()),
            new Money(100, "RUB"),
            CreatedAt);
    }
}
