using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Product.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductApplication(
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