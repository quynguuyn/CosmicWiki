using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Visit.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddVisitInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<VisitDbContext>(options =>
            options.UseOracle(configuration.GetConnectionString("VisitDatabase")
                ?? throw new InvalidOperationException("VisitDatabase connection string is not configured.")));
        services.AddScoped<IVisitUnitOfWork>(sp => sp.GetRequiredService<VisitDbContext>());
        services.AddScoped<IVisitRequestRepository, VisitRequestRepository>();
        services.AddScoped<IExhibitionRepository, ExhibitionRepository>();
        services.AddScoped<IVisitEventRepository, VisitEventRepository>();
        services.AddScoped<IFaqRepository, FaqRepository>();
        return services;
    }
}
