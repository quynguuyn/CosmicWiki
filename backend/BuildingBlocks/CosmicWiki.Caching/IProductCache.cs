namespace CosmicWiki.Caching;

public interface IProductCache
{
    Task<string> GetListKeyAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default,
        Guid? categoryId = null);

    Task InvalidateAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}
