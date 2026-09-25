using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Ordering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
