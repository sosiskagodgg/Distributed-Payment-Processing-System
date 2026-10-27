using Transfer.Application;
using Transfer.Infrastructure;
using Transfer.Infrastructure.Persistence;
using Transfer.Api;
using Transfer.Api.Authentication;
using Transfer.Application.Common;
using Transfer.Infrastructure.Authentication;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var connectionString = builder.Configuration
    .GetConnectionString("TransferDatabase")
    ?? throw new InvalidOperationException(
        "Строка подключения TransferDatabase не найдена.");


builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();


var app = builder.Build();

await using(var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TransferDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapTransferEndpoints();


app.Run();
