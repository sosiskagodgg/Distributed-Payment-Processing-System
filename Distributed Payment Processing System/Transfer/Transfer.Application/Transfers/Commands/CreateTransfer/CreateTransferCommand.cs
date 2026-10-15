using MediatR;
using Transfer.Application.Common;
using Transfer.Domain.Transfers;

namespace Transfer.Application.Transfers.Commands.CreateTransfer;

public record class CreateTransferCommand(
    Guid SenderAccountId,
    Guid RecipientAccountId,
    Guid IdempotencyKey,
    decimal Amount,
    string Currency
    ) : IRequest<Result<TransferId>>;


