

using MediatR;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Application.Common;
using Transfer.Application.DTOs;

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
        var transfer = await _transferQueries.GetByIdAsync(request.Id, cancellationToken);

        if (transfer == null) return Result<TransferDto>.Fail("Перевод не найден");
        return Result<TransferDto>.Ok(transfer);


    }
}

