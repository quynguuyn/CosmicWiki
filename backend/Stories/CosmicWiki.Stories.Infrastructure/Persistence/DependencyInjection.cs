using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Application.Features.AiChat;
using CosmicWiki.Stories.Application.Features.NasaApod;
using CosmicWiki.Stories.Infrastructure.AiChat;
using CosmicWiki.Stories.Infrastructure.Persistence.Repositories;
using CosmicWiki.Stories.Infrastructure.Research;
using CosmicWiki.Stories.Infrastructure.SupportChat;
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
        services.Configure<NasaApodOptions>(
            configuration.GetSection(NasaApodOptions.SectionName));
        services.Configure<AiChatOptions>(
            configuration.GetSection(AiChatOptions.SectionName));
        services.AddHttpClient<CosmicWikiProductCatalogClient>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<AiChatOptions>>()
                .Value;
            client.BaseAddress = new Uri(options.ProductApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddHttpClient<OllamaChatAssistant>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<AiChatOptions>>()
                .Value;
            client.BaseAddress = new Uri(options.OllamaBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(90);
        });
        services.AddTransient<IAiChatAssistant>(provider =>
            provider.GetRequiredService<OllamaChatAssistant>());
        services.AddHttpClient<INasaApodProvider, NasaApodProvider>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<NasaApodOptions>>()
                .Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(
                Math.Clamp(options.RequestTimeoutSeconds, 5, 60));
        });
        services.AddScoped<INasaApodStoryJobScheduler, NasaApodStoryJobScheduler>();
        services.AddScoped<NasaApodStoryJob>();
        services.AddScoped<ISupportChatRepository, SupportChatRepository>();
        return services;
    }
}
