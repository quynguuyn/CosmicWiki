using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Stories.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddStoriesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StoriesDbContext>(options =>
            options.UseOracle(configuration.GetConnectionString("StoriesDatabase")));
        services.AddScoped<IUnitOfWork, StoryUnitOfWork>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        return services;
    }
}
