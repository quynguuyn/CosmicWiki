namespace CosmicWiki.Caching;

public interface IStoryCache
{
    Task<string> GetListKeyAsync(
        string? search,
        string? category,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default);

    string GetDetailKey(Guid storyId);
    string GetSlugKey(string slug);
    Task InvalidateAsync(
        Guid storyId,
        string? previousSlug = null,
        CancellationToken cancellationToken = default);
}
