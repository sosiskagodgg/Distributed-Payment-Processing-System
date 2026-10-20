using Transfer.Domain.Common;
using Transfer.Domain.Transfers;

namespace Transfer.Domain.Tests.Transfers;

public class AccountIdTests
{
    [Fact]
    public void Constructor_WithValidValue_CreatesAccountId()
    {
        var value = Guid.NewGuid();

        var id = new AccountId(value);

        Assert.Equal(value, id.Value);
    }

    [Fact]
    public void Constructor_WithEmptyValue_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new AccountId(Guid.Empty));
    }
}
