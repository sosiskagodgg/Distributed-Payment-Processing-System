using Transfer.Domain.Common;
using Transfer.Domain.Transfers;

namespace Transfer.Domain.Tests.Transfers;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesMoney()
    {
        var money = new Money(100.50m, "rub");

        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("RUB", money.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveAmount_ThrowsDomainException(decimal amount)
    {
        Assert.Throws<DomainException>(() =>
            new Money(amount, "RUB"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyCurrency_ThrowsDomainException(string? currency)
    {
        Assert.Throws<DomainException>(() =>
            new Money(100, currency!));
    }

    [Theory]
    [InlineData("RU")]
    [InlineData("RUBB")]
    public void Constructor_WithInvalidCurrencyLength_ThrowsDomainException(string currency)
    {
        Assert.Throws<DomainException>(() =>
            new Money(100, currency));
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        var first = new Money(100, "RUB");
        var second = new Money(100, "rub");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_WithDifferentAmount_ReturnsFalse()
    {
        var first = new Money(100, "RUB");
        var second = new Money(200, "RUB");

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ReturnsFalse()
    {
        var first = new Money(100, "RUB");
        var second = new Money(100, "USD");

        Assert.NotEqual(first, second);
    }
}
