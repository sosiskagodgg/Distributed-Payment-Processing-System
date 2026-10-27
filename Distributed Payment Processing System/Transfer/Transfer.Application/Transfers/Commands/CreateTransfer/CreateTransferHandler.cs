using Transfer.Domain.Transfers;
using TransferObject = Transfer.Domain.Transfers.Transfer;
using MediatR;
using Transfer.Application.Common;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Domain.Common;
namespace Transfer.Application.Transfers.Commands.CreateTransfer
{
    public class CreateTransferHandler : IRequestHandler<CreateTransferCommand, Result<TransferId>>
    {
        private readonly ITransferRepository _transferRepository;
        private readonly TimeProvider _timeProvider;
        private readonly ICurrentUser _correntUser;
        private readonly IAccountAccessService _accountAccessService;

        public CreateTransferHandler(
            ITransferRepository transferRepository,
            TimeProvider timeProvider,
            ICurrentUser correntUser,
            IAccountAccessService accountAccessService)
        {
            _transferRepository = transferRepository;
            _timeProvider = timeProvider;
            _correntUser = correntUser;
            _accountAccessService = accountAccessService;
        }

        public async Task<Result<TransferId>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
        {
            if(_correntUser.UserId == Guid.Empty)
            {
                return Result<TransferId>.Fail(ErrorType.Unauthorized, "Пользователь не авторизован.");
            }
            if (!await _accountAccessService.CanAccessAccountAsync(_correntUser.UserId,request.SenderAccountId,cancellationToken))
            {
                return Result<TransferId>.Fail(ErrorType.Forbidden, "Невозможно создать перевод с чужого счета.");
            }




            TransferObject transfer;
            try
            {
                transfer = TransferObject.Create(
                    TransferId.New(),
                    new AccountId(request.SenderAccountId),
                    new AccountId(request.RecipientAccountId),
                    new IdempotencyKey(request.IdempotencyKey),
                    new Money(request.Amount, request.Currency),
                    _timeProvider.GetUtcNow()
                    );
            }
            catch (DomainException ex)
            {
                return Result<TransferId>.Fail(ErrorType.Validation, ex.Message);
            }



             CreateOrGetTransferResult result = await _transferRepository.CreateOrGetAsync(transfer, cancellationToken);

            if (!result.WasCreated &&
                (result.Transfer.RecipientAccountId != transfer.RecipientAccountId ||
                result.Transfer.Amount != transfer.Amount))
            {       
                return Result<TransferId>.Fail(ErrorType.Conflict, "Ключ идемпотентности уже использован с другими данными.");
            }


            return Result<TransferId>.Ok(result.Transfer.Id);
            

        }
    }
}
