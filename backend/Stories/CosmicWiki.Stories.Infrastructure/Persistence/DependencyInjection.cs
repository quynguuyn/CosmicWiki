using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Infrastructure.Persistence.Repositories;
using CosmicWiki.Stories.Infrastructure.Research;
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
        services.Configure<NasaApodOptions>(configuration.GetSection(NasaApodOptions.SectionName));
        services.AddHttpClient<INasaApodProvider, NasaApodProvider>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<NasaApodOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.RequestTimeoutSeconds, 5, 60));
        });
        services.AddScoped<NasaApodStoryJob>();
        return services;
    }
}
