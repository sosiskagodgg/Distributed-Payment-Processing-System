
namespace Transfer.Domain.Transfers;

public enum TransferStatus
{
    Created,
    Processing,
    FundsReserved,
    Succeeded,
    Failed,
    Rejected,
    Cancelled
}

