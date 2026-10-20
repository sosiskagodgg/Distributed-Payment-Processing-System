using Transfer.Application;
using Transfer.Infrastructure;
using Transfer.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("TransferDatabase")
    ?? throw new InvalidOperationException(
        "Строка подключения TransferDatabase не найдена.");


builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

await using(var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransferDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}



app.Run();
