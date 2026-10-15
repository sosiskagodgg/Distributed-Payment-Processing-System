using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Transfer.Domain.Transfers;
using Transfer.Application.Abstractions.Persistence;
using Transfer.Infrastructure.Common;
using Transfer.Infrastructure.Persistence;
using Transfer.Infrastructure.Persistence.Repositories;
using Transfer.Infrastructure.Persistence.Queries;
namespace Transfer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))  throw new InfrastructureException("Строка подключения не может быть пустой");
        services.AddDbContext<TransferDbContext>(options=>options.UseNpgsql(connectionString));


        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<ITransferQueries,TransferQueries>();
        services.AddSingleton(TimeProvider.System);

        return services;

    }
}

