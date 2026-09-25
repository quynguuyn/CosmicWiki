using CosmicWiki.SharedKernel.Authorization;

namespace CosmicWiki.Visit.Api.Authorization;

public static class VisitAuthorizationExtensions
{
    public static IServiceCollection AddVisitAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Permissions.VisitRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("permission", Permissions.VisitRead);
            });
            options.AddPolicy(Permissions.VisitWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("permission", Permissions.VisitWrite);
            });
        });
        return services;
    }
}
