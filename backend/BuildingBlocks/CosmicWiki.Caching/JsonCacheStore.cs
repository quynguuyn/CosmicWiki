using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Caching;

internal sealed class JsonCacheStore(
    IDistributedCache cache,
    ILogger<JsonCacheStore> logger) : ICacheStore
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(key, cancellationToken);
        if (payload is null)
        {
            logger.LogDebug("Cache miss for key: {CacheKey}", key);
            return default;
        }

        logger.LogDebug("Cache hit for key: {CacheKey}", key);
        return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value, SerializerOptions);
        return cache.SetStringAsync(
            key,
            payload,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lifetime
            },
            cancellationToken);
    }

    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(key, cancellationToken);
}
