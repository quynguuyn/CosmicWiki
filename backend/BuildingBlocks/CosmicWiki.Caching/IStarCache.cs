namespace CosmicWiki.Caching;

public interface IStarCache
{
    Task<string> GetListKeyAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task InvalidateListAsync(
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(
        Guid starId,
        CancellationToken cancellationToken = default);
}
