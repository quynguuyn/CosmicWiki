using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Infrastructure.Persistence.Outbox;
using CosmicWiki.Product.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Product.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(
            options =>
            {
                options.UseOracle(
                    configuration.GetConnectionString(
                        "ProductDatabase"));
            });

        services.AddScoped<IUnitOfWork, ProductUnitOfWork>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        services.AddScoped<IOutboxWriter, ProductOutboxWriter>();
        
        services.AddHostedService<ProductOutboxPublisher>();

        return services;
    }
}