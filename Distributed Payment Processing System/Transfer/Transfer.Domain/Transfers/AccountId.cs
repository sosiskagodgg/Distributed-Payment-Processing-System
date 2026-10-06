using Transfer.Domain.Common;

namespace Transfer.Domain.Transfers;

public readonly record struct AccountId
{
    public Guid Value { get; }
    public AccountId(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("AccountId не может быть пустым.");
        Value = value;
    }
}

