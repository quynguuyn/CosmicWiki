namespace CosmicWiki.Caching;

public interface ICategoryCache
{
    Task<string> GetListKeyAsync(
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
