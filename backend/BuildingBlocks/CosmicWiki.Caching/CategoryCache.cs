namespace CosmicWiki.Caching;

public interface ICategoryCache
{
    Task<string> GetListKeyAsync(
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
}

internal sealed class CategoryCache(ICacheStore store) : ICategoryCache
{
    private const string ListVersionKey = "category:list:version";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public async Task<string> GetListKeyAsync(
        CancellationToken cancellationToken = default)
    {
        var version = await store.GetAsync<string>(
            ListVersionKey,
            cancellationToken);

        if (version is null)
        {
            version = Guid.NewGuid().ToString("N");
            await store.SetAsync(
                ListVersionKey,
                version,
                Lifetime,
                cancellationToken);
        }

        return $"category:list:{version}";
    }

    public Task InvalidateAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return InvalidateCoreAsync(categoryId, cancellationToken);
    }

    private async Task InvalidateCoreAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        await store.RemoveAsync(
            $"category:{categoryId:N}",
            cancellationToken);
            
        await store.SetAsync(
            ListVersionKey,
            Guid.NewGuid().ToString("N"),
            Lifetime,
            cancellationToken);
    }
}
