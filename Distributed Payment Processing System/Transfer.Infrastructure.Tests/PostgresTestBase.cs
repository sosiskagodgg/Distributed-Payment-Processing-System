using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Transfer.Domain.Transfers;
using Transfer.Infrastructure.Persistence;
using TransferAggregate = Transfer.Domain.Transfers.Transfer;

namespace Transfer.Infrastructure.Tests;

public abstract class PostgresTestBase : IAsyncLifetime
{
    private static readonly SemaphoreSlim DatabaseCreationLock = new(1, 1);
    protected static readonly DateTimeOffset CreatedAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly string _schema = $"transfer_tests_{Guid.NewGuid():N}";
    private string _connectionString = null!;
    private bool _schemaCreated;

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__TransferTestDatabase")
            ?? configuration.GetConnectionString("TransferTestDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__TransferTestDatabase or configure TransferTestDatabase in appsettings.json.");

        _connectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SearchPath = _schema,
            Pooling = false
        }.ConnectionString;

        await using var context = CreateDbContext();
        await DatabaseCreationLock.WaitAsync();
        try
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();
            if (!await creator.ExistsAsync())
                await creator.CreateAsync();
        }
        finally
        {
            DatabaseCreationLock.Release();
        }

        await ExecuteSchemaCommandAsync($"CREATE SCHEMA \"{_schema}\"");
        _schemaCreated = true;
        try
        {
            await context.Database.ExecuteSqlRawAsync(context.Database.GenerateCreateScript());
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (!_schemaCreated)
            return;

        await ExecuteSchemaCommandAsync($"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE");
        _schemaCreated = false;
    }

    private async Task ExecuteSchemaCommandAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    protected TransferDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransferDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new TransferDbContext(options);
    }

    protected static TransferAggregate GetTransfer(
        TransferId? transferId = null,
        AccountId? senderAccountId = null,
        IdempotencyKey? idempotencyKey = null,
        AccountId? recipientAccountId = null,
        Money? amount = null)
        => TransferAggregate.Create(
            transferId ?? TransferId.New(),
            senderAccountId ?? new AccountId(Guid.NewGuid()),
            recipientAccountId ?? new AccountId(Guid.NewGuid()),
            idempotencyKey ?? new IdempotencyKey(Guid.NewGuid()),
            amount ?? new Money(100.25m, "RUB"),
            CreatedAt);

    protected async Task SeedAsync(TransferAggregate transfer)
    {
        await using var context = CreateDbContext();
        context.Transfers.Add(transfer);
        await context.SaveChangesAsync();
    }
}
