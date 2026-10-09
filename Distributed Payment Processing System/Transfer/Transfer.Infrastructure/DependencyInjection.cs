using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Transfer.Domain.Transfers;
using Transfer.Infrastructure.Common;
using Transfer.Infrastructure.Persistence;
namespace Transfer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))  throw new InfrastructureException("Строка подключения не может быть пустой");
        services.AddDbContext<TransferDbContext>(options=>options.UseNpgsql(connectionString));



        return services;

    }
}

