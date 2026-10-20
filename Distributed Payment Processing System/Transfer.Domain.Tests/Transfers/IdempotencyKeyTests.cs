using Transfer.Domain.Common;
using Transfer.Domain.Transfers;

namespace Transfer.Domain.Tests.Transfers;

public class IdempotencyKeyTests
{
    [Fact]
    public void Constructor_WithValidValue_CreatesIdempotencyKey()
    {
        var value = Guid.NewGuid();

        var key = new IdempotencyKey(value);

        Assert.Equal(value, key.Value);
    }

    [Fact]
    public void Constructor_WithEmptyValue_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new IdempotencyKey(Guid.Empty));
    }
}
