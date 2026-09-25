namespace CosmicWiki.Caching;

public interface ICacheStore
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
