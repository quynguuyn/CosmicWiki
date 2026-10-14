using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;

namespace CosmicWiki.Product.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<ProductEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProductEntity?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductEntity>> GetPageAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ProductEntity product,
        CancellationToken cancellationToken = default);

    void Remove(ProductEntity product);
}