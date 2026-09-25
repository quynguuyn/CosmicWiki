namespace CosmicWiki.Caching;

internal sealed class NoOpCacheStore : ICacheStore
{
    public Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<T?>(default);

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
