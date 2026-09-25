using CosmicWiki.SharedKernel.Authorization;

namespace CosmicWiki.Product.Api.Authorization;

public static class ProductAuthorizationExtensions
{
    public static IServiceCollection AddProductAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Permissions.ProductRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.ProductRead);
                });

            options.AddPolicy(
                Permissions.ProductWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.ProductWrite);
                });
        });

        return services;
    }
}