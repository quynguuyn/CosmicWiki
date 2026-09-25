using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Auth.Api.Authorization;

public static class AuthAuthorizationExtensions
{
    public static IServiceCollection AddAuthAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Permissions.AuthRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.AuthRead);
                });

            options.AddPolicy(
                Permissions.AuthWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(
                        "permission",
                        Permissions.AuthWrite);
                });
        });

        return services;
    }
}