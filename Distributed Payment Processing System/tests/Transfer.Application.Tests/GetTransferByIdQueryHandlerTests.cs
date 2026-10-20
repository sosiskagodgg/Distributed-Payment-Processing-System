
using Transfer.Application.Abstractions.Persistence;
using Transfer.Application.Common;
using Transfer.Application.DTOs;
using Transfer.Application.Transfers.Queries.GetTransferById;
using Transfer.Domain.Transfers;


namespace Transfer.Application.Tests;

public class GetTransferByIdQueryHandlerTests
{


    [Fact]
    public async Task Handle_ExistingTransfer_ReturnsTransferDtoGetTransferByIdQueryHandlerTests()
    {
        Guid transferId = Guid.NewGuid();
        TransferDto transferDto = new TransferDto(
            transferId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB",
            "created",
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            ""
            );

        TestQueries testQueries = new TestQueries(transferDto);

        GetTransferByIdQueryHandler handler = new(testQueries);

        var result = await handler.Handle(
            new GetTransferByIdQuery(transferId), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(result.Value, transferDto);




    }
    [Fact]
    public async Task Handle_MissingTransfer_ReturnsNotFound()
    {
        Guid transferId = Guid.NewGuid();


        TestQueries testQueries = new TestQueries();

        GetTransferByIdQueryHandler handler = new(testQueries);

        var result = await handler.Handle(
            new GetTransferByIdQuery(transferId), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Null(result.Value);




    }
    [Fact]
    public async Task Handle_ValidQuery_PassesIdAndCancellationTokenToQueries()
    {
        Guid transferId = Guid.NewGuid();
        TransferDto transferDto = new TransferDto(
            transferId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            "RUB",
            "created",
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            ""
            );

        TestQueries testQueries = new TestQueries(transferDto);

        GetTransferByIdQueryHandler handler = new(testQueries);

        CancellationToken cancellationToken = new();


        var result = await handler.Handle(
            new GetTransferByIdQuery(transferId), cancellationToken);

        Assert.Equal(cancellationToken, testQueries.LastCancellationToken);
        Assert.Equal(transferId, testQueries.LastRequestedId);




    }


    private class TestQueries : ITransferQueries
    {
        private readonly List<TransferDto> transfers;
        public Guid LastRequestedId { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }
        public TestQueries(params TransferDto[] transfers)
        {
            this.transfers = new List<TransferDto>(transfers);
        }

        public Task<TransferDto?> GetByIdAsync(TransferId id, CancellationToken cancellationToken)
        {
            LastRequestedId = id.Value;
            LastCancellationToken = cancellationToken;

            cancellationToken.ThrowIfCancellationRequested();

            var transfer = transfers.FirstOrDefault(t => t.Id == id.Value);
            return Task.FromResult(transfer);
        }
    }
}

