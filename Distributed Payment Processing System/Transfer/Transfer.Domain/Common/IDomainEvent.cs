using System;
using System.Collections.Generic;
using System.Text;

namespace Transfer.Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredAt { get; }
}
