namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IProductSnapshotProvider
{
    Task<ProductSnapshot?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken);
}

public sealed record ProductSnapshot(
    Guid ProductId,
    string SKU,
    string Name,
    decimal UnitPrice,
    int AvailableStock,
    bool IsActive);
