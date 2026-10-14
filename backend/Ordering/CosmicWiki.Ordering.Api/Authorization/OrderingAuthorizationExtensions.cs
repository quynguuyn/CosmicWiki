using CosmicWiki.SharedKernel.Authorization;

namespace CosmicWiki.Ordering.Api.Authorization;

public static class OrderingAuthorizationExtensions
{
    public static IServiceCollection AddOrderingAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Permissions.OrderingWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("permission", Permissions.OrderingWrite);
                });
            options.AddPolicy(
                Permissions.ShippingWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("permission", Permissions.ShippingWrite);
                });
        });

        return services;
    }
}
