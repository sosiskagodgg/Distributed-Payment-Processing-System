using Transfer.Application.Abstractions.Persistence;
using Transfer.Application.Common;
using Transfer.Application.Transfers.Commands;
using Transfer.Application.Transfers.Commands.CreateTransfer;
using Transfer.Domain.Transfers;
namespace Transfer.Application.Tests;

public class CreateTransferHandlerTests
{



    [Fact]

    public async Task Handle_ValidCommand_ReturnsCreatedTransferId()
    {


        ITransferRepository _transferRepository = new TestRepository();
        CreateTransferCommand createTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB");

        var result = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(createTransferCommand,default);

        var transfer = await _transferRepository.GetByIdAsync(result.Value);



        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Value);
        Assert.NotNull(transfer);


    }

    [Fact]
    public async Task Handle_InvalidDomainData_ReturnsValidationError()
    {

        CreateTransferCommand createTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            -100,
            "RUB");





        var id = await new CreateTransferHandler(
            new TestRepository(),
            TimeProvider.System).Handle(createTransferCommand, default);

        Assert.Equal(ErrorType.Validation, id.ErrorType);
    }


    [Fact]
    public async Task Handle_ExistingTransferWithSameData_ReturnsExistingTransferId()
    {
        ITransferRepository _transferRepository = new TestRepository();
        CreateTransferCommand createTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB");





        var firstId = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(createTransferCommand, default);

        var secondId = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(createTransferCommand, default);
        Assert.True(firstId.IsSuccess);
        Assert.True(secondId.IsSuccess);

        Assert.Equal(firstId.Value, secondId.Value);
    }

    [Fact]
    public async Task Handle_ExistingTransferWithDifferentRecipient_ReturnsConflict()
    {
        ITransferRepository _transferRepository = new TestRepository();
        CreateTransferCommand firstCreateTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB");

        CreateTransferCommand secondCreateTransferCommand = new(
            firstCreateTransferCommand.SenderAccountId,
            Guid.NewGuid(),
            firstCreateTransferCommand.IdempotencyKey,
            100,
            "RUB");





        var firstResult = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(firstCreateTransferCommand, default);

        var secondResult = await new CreateTransferHandler(
             _transferRepository,
             TimeProvider.System).Handle(secondCreateTransferCommand, default);

        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);
        Assert.Equal(ErrorType.Conflict, secondResult.ErrorType);
    }

    [Fact]
    public async Task Handle_ExistingTransferWithDifferentAmount_ReturnsConflict()
    {
        ITransferRepository _transferRepository = new TestRepository();
        CreateTransferCommand firstCreateTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB");

        CreateTransferCommand secondCreateTransferCommand = new(
            firstCreateTransferCommand.SenderAccountId,
            firstCreateTransferCommand.RecipientAccountId,
            firstCreateTransferCommand.IdempotencyKey,
            1000,
            "RUB");





        var firstResult = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(firstCreateTransferCommand, default);

        var secondResult = await new CreateTransferHandler(
             _transferRepository,
             TimeProvider.System).Handle(secondCreateTransferCommand, default);

        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);
        Assert.Equal(ErrorType.Conflict, secondResult.ErrorType);
    }
    [Fact]
    public async Task Handle_ExistingTransferWithDifferentCurrency_ReturnsConflict()
    {
        ITransferRepository _transferRepository = new TestRepository();
        CreateTransferCommand firstCreateTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB");

        CreateTransferCommand secondCreateTransferCommand = new(
            firstCreateTransferCommand.SenderAccountId,
            firstCreateTransferCommand.RecipientAccountId,
            firstCreateTransferCommand.IdempotencyKey,
            100,
            "USD");





        var firstResult = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(firstCreateTransferCommand, default);

        var secondResult = await new CreateTransferHandler(
             _transferRepository,
             TimeProvider.System).Handle(secondCreateTransferCommand, default);

        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);
        Assert.Equal(ErrorType.Conflict, secondResult.ErrorType);
    }

    [Fact]
    public async Task Handle_SameIdempotencyKeyForDifferentSenders_CreatesSeparateTransfers()
    {
        ITransferRepository _transferRepository = new TestRepository();
        CreateTransferCommand firstCreateTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB");

        CreateTransferCommand secondCreateTransferCommand = new(
            Guid.NewGuid(),
            firstCreateTransferCommand.RecipientAccountId,
            firstCreateTransferCommand.IdempotencyKey,
            100,
            "RUB");





        var firstResult = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(firstCreateTransferCommand, default);

        var secondResult = await new CreateTransferHandler(
             _transferRepository,
             TimeProvider.System).Handle(secondCreateTransferCommand, default);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.NotEqual(firstResult.Value,secondResult.Value);
    }

    [Fact]
    public async Task Handle_InvalidDomainData_DoesNotCallRepository()
    {
        TestRepository _transferRepository = new TestRepository();
        CreateTransferCommand createTransferCommand = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            -100,
            "RUB");
        




        var result = await new CreateTransferHandler(
            _transferRepository,
            TimeProvider.System).Handle(createTransferCommand, default);

        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Equal(0, _transferRepository.CountCallCreateOrGet);

    }





    public class TestRepository : ITransferRepository
    {
        private readonly List<Domain.Transfers.Transfer> transfers = new();

        public int CountCallCreateOrGet { get;private set; }
        public Task<bool> AnyAsync(TransferId transferId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(transfers.Any(t => t.Id == transferId));
        }

        public Task<CreateOrGetTransferResult> CreateOrGetAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existing = transfers.FirstOrDefault(t =>
                t.SenderAccountId == transfer.SenderAccountId &&
                t.IdempotencyKey == transfer.IdempotencyKey);


            CountCallCreateOrGet++;

            if (existing is not null)
                return Task.FromResult(new CreateOrGetTransferResult(existing, false));

            transfers.Add(transfer);
            return Task.FromResult(new CreateOrGetTransferResult(transfer, true));
        }

        public Task<Domain.Transfers.Transfer?> GetByIdAsync(TransferId transferId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(transfers.FirstOrDefault(t => t.Id == transferId));
        }

        public Task<Domain.Transfers.Transfer?> GetByIdempotencyKeyAsync(AccountId senderAccountId, IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(transfers.FirstOrDefault(t =>
                t.SenderAccountId == senderAccountId &&
                t.IdempotencyKey == idempotencyKey));
        }

        public Task UpdateAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var index = transfers.FindIndex(t => t.Id == transfer.Id);
            if (index < 0)
                throw new InvalidOperationException("Перевод не найден в тестовом репозитории.");

            transfers[index] = transfer;
            return Task.CompletedTask;
        }
    }
}
