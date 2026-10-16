using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Transfer.Application.Common.Behaviors;
namespace Transfer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(
                typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(
                typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}

