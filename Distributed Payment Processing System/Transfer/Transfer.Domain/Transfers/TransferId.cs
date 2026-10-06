using Transfer.Domain.Common;

namespace Transfer.Domain.Transfers;

public readonly record struct TransferId
{
    public Guid Value { get; }

    public TransferId(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("TransferId не может быть пустым.");

        Value = value;
    }

    public static TransferId New() => new(Guid.NewGuid());
}

