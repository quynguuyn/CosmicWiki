using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Stories.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStoriesApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
