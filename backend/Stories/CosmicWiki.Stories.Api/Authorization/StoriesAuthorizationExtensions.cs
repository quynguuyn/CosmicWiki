using CosmicWiki.SharedKernel.Authorization;

namespace CosmicWiki.Stories.Api.Authorization;

public static class StoriesAuthorizationExtensions
{
    public static IServiceCollection AddStoriesAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Permissions.StoryRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("permission", Permissions.StoryRead);
            });
            options.AddPolicy(Permissions.StoryWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("permission", Permissions.StoryWrite);
            });
        });
        return services;
    }
}
