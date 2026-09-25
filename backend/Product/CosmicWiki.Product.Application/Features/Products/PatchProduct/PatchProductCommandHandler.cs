using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Product;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.PatchProduct;

public sealed class PatchProductCommandHandler
    : IRequestHandler<PatchProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public PatchProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IProductCache productCache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _productCache = productCache;
    }

    public async Task Handle(
        PatchProductCommand request,
        CancellationToken cancellationToken)
    {
        var product =
            await _productRepository.GetByIdForUpdateAsync(
                request.Id,
                cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product '{request.Id}' was not found.");
        }

        var sku = request.SKU ?? product.SKU;
        var name = request.Name ?? product.Name;
        var slug = request.Slug ?? product.Slug;
        var description =
            request.Description ?? product.Description;
        var originalPrice =
            request.OriginalPrice ?? product.OriginalPrice;
        var salePrice =
            request.SalePrice ?? product.SalePrice;
        var stockQuantity =
            request.StockQuantity ?? product.StockQuantity;
        var status =
            request.Status ?? product.Status;
        var categoryId =
            request.CategoryId ?? product.CategoryId;

        sku = sku.Trim();
        slug = slug.Trim();

        if (!string.Equals(
                product.SKU,
                sku,
                StringComparison.OrdinalIgnoreCase) &&
            await _productRepository.ExistsBySkuAsync(
                sku,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Product SKU '{sku}' already exists.");
        }

        if (!string.Equals(
                product.Slug,
                slug,
                StringComparison.OrdinalIgnoreCase) &&
            await _productRepository.ExistsBySlugAsync(
                slug,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Product slug '{slug}' already exists.");
        }

        if (categoryId != product.CategoryId &&
            !await _categoryRepository.ExistsAsync(
                categoryId,
                cancellationToken))
        {
            throw new KeyNotFoundException(
                $"Category '{categoryId}' was not found.");
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

        _outboxWriter.Add(
            new ProductUpdatedIntegrationEvent(
                Guid.NewGuid(), 1, DateTime.UtcNow, product.Id, product.Id, product.SKU,
                product.Name, product.Slug, product.Description,
                product.Images.OrderBy(x => x.DisplayOrder).Select(x => x.Url).FirstOrDefault()),
            KafkaTopics.ProductUpdated);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
        await _productCache.InvalidateAsync(
            product.Id,
            cancellationToken);
    }
}
