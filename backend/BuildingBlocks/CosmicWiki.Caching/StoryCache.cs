namespace CosmicWiki.Caching;

internal sealed class StoryCache(ICacheStore store) : IStoryCache
{
    private const string ListVersionKey = "story:list:version";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public async Task<string> GetListKeyAsync(
        string? search,
        string? category,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default)
    {
        var version = await store.GetAsync<string>(ListVersionKey, cancellationToken);
        if (version is null)
        {
            version = Guid.NewGuid().ToString("N");
            await store.SetAsync(ListVersionKey, version, Lifetime, cancellationToken);
        }

        return $"story:list:{version}:{Encode(search)}:{Encode(category)}:{page}:{pageSize}:{Encode(sort)}";
    }

    public string GetDetailKey(Guid storyId) => $"story:{storyId:N}";

    public string GetSlugKey(string slug) => $"story:slug:{Encode(slug)}";

    public async Task InvalidateAsync(
        Guid storyId,
        string? previousSlug = null,
        CancellationToken cancellationToken = default)
    {
        await store.RemoveAsync(GetDetailKey(storyId), cancellationToken);
        if (!string.IsNullOrWhiteSpace(previousSlug))
            await store.RemoveAsync(GetSlugKey(previousSlug), cancellationToken);
        await store.SetAsync(
            ListVersionKey,
            Guid.NewGuid().ToString("N"),
            Lifetime,
            cancellationToken);
    }

    private static string Encode(string? value) =>
        Uri.EscapeDataString(value ?? string.Empty);
}
