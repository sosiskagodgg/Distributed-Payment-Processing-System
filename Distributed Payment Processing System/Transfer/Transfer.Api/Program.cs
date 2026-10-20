using Transfer.Application;
using Transfer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("TransferDatabase")
    ?? throw new InvalidOperationException(
        "Строка подключения TransferDatabase не найдена.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.Run();
