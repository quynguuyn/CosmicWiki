namespace CosmicWiki.Caching;

internal sealed class ProductCache(ICacheStore store) : IProductCache
{
    private const string ListVersionKey = "product:list:version";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public async Task<string> GetListKeyAsync(
        int page,
        int pageSize,
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

        return $"product:list:{version}:{page}:{pageSize}";
    }

    public Task InvalidateAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return InvalidateCoreAsync(productId, cancellationToken);
    }

    private async Task InvalidateCoreAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        await store.RemoveAsync(
            $"product:{productId:N}",
            cancellationToken);
        await store.SetAsync(
            ListVersionKey,
            Guid.NewGuid().ToString("N"),
            Lifetime,
            cancellationToken);
    }
}
