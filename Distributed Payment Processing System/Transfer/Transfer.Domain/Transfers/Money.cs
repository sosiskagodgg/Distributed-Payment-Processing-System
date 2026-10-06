using Transfer.Domain.Common;
namespace Transfer.Domain.Transfers;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Валюта не может быть пустой");
        if (amount <= 0) throw new DomainException($"Количество средств не может быть равно или меньше нуля {amount}");
        this.Amount = amount;
        this.Currency = currency.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

