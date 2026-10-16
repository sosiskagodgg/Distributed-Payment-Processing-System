using Transfer.Domain.Transfers;

namespace Transfer.Application.Abstractions.Persistence;

public sealed record CreateOrGetTransferResult(
    Transfer.Domain.Transfers.Transfer Transfer,
    bool WasCreated);
