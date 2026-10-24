using Transfer.Application.DTOs;
using Transfer.Domain.Transfers;
using Transfer.Infrastructure.Persistence.Queries;

namespace Transfer.Infrastructure.Tests.Transfer;

public class TransferQueriesTests : PostgresTestBase
{
    [Fact]
    public async Task GetByIdAsync_ExistingTransfer_ReturnsDtoWithCorrectFields()
    {
        var transfer = GetTransfer();
        transfer.StartProcessing(CreatedAt.AddMinutes(1));
        transfer.Fail("Insufficient funds", CreatedAt.AddMinutes(2));
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var queries = new TransferQueries(context);
        var expected = new TransferDto(
            transfer.Id.Value,
            transfer.SenderAccountId.Value,
            transfer.RecipientAccountId.Value,
            100.25m,
            "RUB",
            "Failed",
            CreatedAt,
            CreatedAt.AddMinutes(2),
            "Insufficient funds");

        var result = await queries.GetByIdAsync(transfer.Id, CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_MissingTransfer_ReturnsNull()
    {
        await SeedAsync(GetTransfer());
        await using var context = CreateDbContext();
        var queries = new TransferQueries(context);

        Assert.Null(await queries.GetByIdAsync(TransferId.New(), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTransfer_DoesNotTrackEntity()
    {
        var transfer = GetTransfer();
        await SeedAsync(transfer);
        await using var context = CreateDbContext();
        var queries = new TransferQueries(context);

        var result = await queries.GetByIdAsync(transfer.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
