using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Infrastructure.Persistence.Repositories;
using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Infrastructure.Research;
using CosmicWiki.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Catalog.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseOracle(
                configuration.GetConnectionString("CatalogDatabase"));
        });
    
        services.AddScoped<ICatalogUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
        
        services.AddScoped<IStarRepository, StarRepository>();
        
        services.AddScoped<IAstronomicalObjectRepository, AstronomicalObjectRepository>();
        
        services.AddScoped<IAstronomicalImageRepository, AstronomicalImageRepository>();
        services.AddScoped<IOutboxWriter, CatalogOutboxWriter>();
        services.AddHostedService<CatalogOutboxPublisher>();
        services.Configure<AstronomicalResearchOptions>(
            configuration.GetSection("AstronomicalResearch"));
        services.AddHttpClient<IAstronomicalResearchProvider, SimBadAstronomicalResearchProvider>(
            client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IAstronomicalResearchJob, AstronomicalResearchJob>();

        return services;
    }
}
