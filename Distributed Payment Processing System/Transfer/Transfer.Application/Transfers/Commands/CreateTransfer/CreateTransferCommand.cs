using MediatR;
using System.Windows.Input;

namespace Transfer.Application.Transfers.Commands.CreateTransfer;

public record class CreateTransferCommand(
    Guid senderAccountId,
    Guid recipientAccountId,
    Guid idempotencyKey,
    decimal amount,
    string currency
    ) : IRequest<Guid>;


