using System;
using System.Collections.Generic;
using System.Text;
using Transfer.Domain.Common;

namespace Transfer.Domain.Transfers;

public readonly record struct IdempotencyKey
{
    public Guid Value { get; }
    public IdempotencyKey(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("IdempotencyKey не может быть пустым.");
        Value = value;
    }
}
