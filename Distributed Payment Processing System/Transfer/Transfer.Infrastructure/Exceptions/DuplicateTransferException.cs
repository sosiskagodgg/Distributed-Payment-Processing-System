using Transfer.Infrastructure.Common;

namespace Transfer.Infrastructure.Exceptions;

public class DuplicateTransferException : InfrastructureException
{
    public DuplicateTransferException(string message) : base(message)
    {
    }
}

