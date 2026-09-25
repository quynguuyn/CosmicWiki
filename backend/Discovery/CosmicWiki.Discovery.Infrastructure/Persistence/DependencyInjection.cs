using CosmicWiki.Discovery.Application.Abstractions.Persistence;
using CosmicWiki.Discovery.Infrastructure.Persistence.Repositories;
using CosmicWiki.Discovery.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Discovery.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscoveryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DiscoveryDbContext>(
            options =>
            {
                options.UseOracle(
                    configuration.GetConnectionString(
                        "DiscoveryDatabase"));
            });

        services.AddScoped<ISearchDocumentRepository,
            SearchDocumentRepository>();

        services.AddHostedService<ProductSearchDocumentConsumer>();
        services.AddHostedService<StarSearchDocumentConsumer>();

        return services;
    }
}
