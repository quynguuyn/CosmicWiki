using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Catalog.Api.Authorization;

public static class CatalogAuthorizationExtensions
{
    public static IServiceCollection AddCatalogAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Permissions.CatalogRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.CatalogRead);
                });

            options.AddPolicy(
                Permissions.CatalogWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.CatalogWrite);
                });
        });

        return services;
    }
}