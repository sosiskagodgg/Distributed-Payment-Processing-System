using Transfer.Domain.Transfers;
using TransferObject = Transfer.Domain.Transfers.Transfer;
using MediatR;
using Transfer.Application.Common;
namespace Transfer.Application.Transfers.Commands.CreateTransfer
{
    public class CreateTrasferHandler : IRequestHandler<CreateTransferCommand, Result<TransferId>>
    {
        ITransferRepository _transferRepository;
        TimeProvider _timeProvider;


        public CreateTrasferHandler(ITransferRepository transferRepository,TimeProvider timeProvider)
        {
            _transferRepository = transferRepository;
            _timeProvider = timeProvider;
        }

        public async Task<Result<TransferId>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
        {
            TransferObject transfer;
            try
            {
                transfer = TransferObject.Create(
                    new TransferId(Guid.NewGuid()),
                    new AccountId(request.senderAccountId),
                    new AccountId(request.recipientAccountId),
                    new IdempotencyKey(request.idempotencyKey),
                    new Money(request.amount, request.currency),
                    _timeProvider.GetUtcNow()
                    );
            }
            catch (Exception ex)
            {
                return Result<TransferId>.Fail(ex.ToString());
            }
            if(await _transferRepository.GetByIdempotencyKeyAsync(transfer.SenderAccountId, transfer.IdempotencyKey)==null)
                await _transferRepository.AddAsync(transfer, cancellationToken);
            else
            {
                return Result<TransferId>.Fail("Операция с таким же ключем индентичности и акаунтом отправителя уже была создана");
            }
            return Result<TransferId>.Ok(transfer.Id);

        }
    }
}
