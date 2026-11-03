using CosmicWiki.Caching;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Domain.Enums;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;

namespace CosmicWiki.Product.Application.Features.Products.Common;

internal static class ProductUpdateWorkflow
{
    public static async Task ApplyAsync(
        ProductEntity product,
        string sku,
        string name,
        string slug,
        string? description,
        decimal originalPrice,
        decimal salePrice,
        int stockQuantity,
        Guid categoryId,
        ProductStatus status,
        bool validateCategory,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IProductCache productCache,
        CancellationToken cancellationToken)
    {
        sku = sku.Trim();
        slug = slug.Trim();

        if (!string.Equals(product.SKU, sku, StringComparison.OrdinalIgnoreCase) &&
            await productRepository.ExistsBySkuAsync(sku, cancellationToken))
        {
            throw new InvalidOperationException($"Product SKU '{sku}' already exists.");
        }

        if (!string.Equals(product.Slug, slug, StringComparison.OrdinalIgnoreCase) &&
            await productRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            throw new InvalidOperationException($"Product slug '{slug}' already exists.");
        }

        if (validateCategory &&
            !await categoryRepository.ExistsAsync(categoryId, cancellationToken))
        {
            throw new KeyNotFoundException($"Category '{categoryId}' was not found.");
        }

        product.Update(
            sku,
            name,
            slug,
            description,
            originalPrice,
            salePrice,
            stockQuantity,
            categoryId,
            status);

        await SaveAndInvalidateAsync(
            product,
            unitOfWork,
            outboxWriter,
            productCache,
            cancellationToken);
    }

    public static async Task SaveAndInvalidateAsync(
        ProductEntity product,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IProductCache productCache,
        CancellationToken cancellationToken)
    {
        ProductIndexingEvents.AddUpdated(outboxWriter, product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await productCache.InvalidateAsync(product.Id, cancellationToken);
    }
}
