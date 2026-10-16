using MediatR;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Application.Common;
using Transfer.Application.DTOs;
using Transfer.Domain.Transfers;

namespace Transfer.Application.Transfers.Queries.GetTransferById;

public class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, Result<TransferDto>>
{
    private readonly ITransferQueries _transferQueries;

    public GetTransferByIdQueryHandler(ITransferQueries transferQueries)
    {
        _transferQueries = transferQueries;
    }

    public async Task<Result<TransferDto>> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _transferQueries.GetByIdAsync(new TransferId(request.Id), cancellationToken);

        if (transfer == null) return Result<TransferDto>.Fail(ErrorType.NotFound,"Перевод не найден");
        return Result<TransferDto>.Ok(transfer);


    }
}

