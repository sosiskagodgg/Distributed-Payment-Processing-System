

using MediatR;
using System.Windows.Input;
using Transfer.Application.Common;
using Transfer.Application.DTOs;
using Transfer.Domain.Transfers;

namespace Transfer.Application.Transfers.Queries.GetTransferById;

public sealed record GetTransferByIdQuery(TransferId Id) : IRequest<Result<TransferDto>>;


