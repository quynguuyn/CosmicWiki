using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Discovery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscoveryApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}