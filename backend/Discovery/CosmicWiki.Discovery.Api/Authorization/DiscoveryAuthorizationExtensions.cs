using CosmicWiki.SharedKernel.Authorization;

namespace CosmicWiki.Discovery.Api.Authorization;

public static class DiscoveryAuthorizationExtensions
{
    public static IServiceCollection AddDiscoveryAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Permissions.DiscoveryRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.DiscoveryRead);
                });

            options.AddPolicy(
                Permissions.DiscoveryWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.DiscoveryWrite);
                });
        });

        return services;
    }
}