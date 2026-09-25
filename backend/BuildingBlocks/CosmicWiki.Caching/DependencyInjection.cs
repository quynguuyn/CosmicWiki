using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddCosmicWikiCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<CachingOptions>()
            .Bind(configuration.GetSection(CachingOptions.SectionName));

        var options = configuration
            .GetSection(CachingOptions.SectionName)
            .Get<CachingOptions>() ?? new CachingOptions();

        if (!options.Enabled ||
            string.IsNullOrWhiteSpace(options.Redis.ConnectionString))
        {
            services.AddSingleton<ICacheStore, NoOpCacheStore>();
            services.AddSingleton<IProductCache, ProductCache>();
            services.AddSingleton<IStarCache, StarCache>();
            return services;
        }

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = options.Redis.ConnectionString;
            redisOptions.InstanceName = options.Redis.InstanceName;
        });
        services.AddSingleton<ICacheStore, JsonCacheStore>();
        services.AddSingleton<IProductCache, ProductCache>();
        services.AddSingleton<IStarCache, StarCache>();
        services.AddSingleton<ICategoryCache, CategoryCache>();
        return services;
    }
}
