using Transfer.Domain.Common;
using Transfer.Domain.Transfers;

namespace Transfer.Domain.Tests.Transfers;

public class TransferIdTests
{
    [Fact]
    public void Constructor_WithValidValue_CreatesTransferId()
    {
        var value = Guid.NewGuid();

        var id = new TransferId(value);

        Assert.Equal(value, id.Value);
    }

    [Fact]
    public void Constructor_WithEmptyValue_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new TransferId(Guid.Empty));
    }

    [Fact]
    public void New_CreatesNonEmptyUniqueIdentifiers()
    {
        var first = TransferId.New();
        var second = TransferId.New();

        Assert.NotEqual(Guid.Empty, first.Value);
        Assert.NotEqual(first, second);
    }
}
