using MediatR;
using Microsoft.Extensions.DependencyInjection;
namespace CosmicWiki.Visit.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddVisitApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
