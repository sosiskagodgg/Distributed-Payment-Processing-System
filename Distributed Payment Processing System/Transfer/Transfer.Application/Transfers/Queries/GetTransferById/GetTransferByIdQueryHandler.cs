using MediatR;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Application.Common;
using Transfer.Application.DTOs;
using Transfer.Domain.Transfers;

namespace Transfer.Application.Transfers.Queries.GetTransferById;

public class GetTransferByIdQueryHandler : IRequestHandler<GetTransferByIdQuery, Result<TransferDto>>
{
    private readonly ITransferQueries _transferQueries;
    private readonly ICurrentUser _correntUser;
    private readonly IAccountAccessService _accountAccessService;

    public GetTransferByIdQueryHandler(ITransferQueries transferQueries,ICurrentUser correntUser,IAccountAccessService accountAccessService)
    {
        _transferQueries = transferQueries;
        _correntUser = correntUser;
        _accountAccessService = accountAccessService;
    }

    public async Task<Result<TransferDto>> Handle(GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        if (_correntUser.UserId == Guid.Empty)
        {
            return Result<TransferDto>.Fail(ErrorType.Unauthorized, "Пользователь не авторизован.");
        }
       



        var transfer = await _transferQueries.GetByIdAsync(new TransferId(request.Id), cancellationToken);
        if (transfer == null) return Result<TransferDto>.Fail(ErrorType.NotFound,"Перевод не найден");
       
        var userId = _correntUser.UserId;

        bool hasAccess =
            await _accountAccessService.CanAccessAccountAsync(
                userId, transfer.SenderAccountId, cancellationToken)
            ||
            await _accountAccessService.CanAccessAccountAsync(
                userId, transfer.RecipientAccountId, cancellationToken);

        if(!hasAccess)
        {
            return Result<TransferDto>.Fail(ErrorType.NotFound, "Перевод не найден");
        }



        return Result<TransferDto>.Ok(transfer);


    }
}

