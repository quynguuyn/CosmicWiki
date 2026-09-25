using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Product;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public UpdateProductCommandHandler(
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
        UpdateProductCommand request,
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

        var sku = request.SKU.Trim();
        var slug = request.Slug.Trim();

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

        if (!await _categoryRepository.ExistsAsync(
                request.CategoryId,
                cancellationToken))
        {
            throw new KeyNotFoundException(
                $"Category '{request.CategoryId}' was not found.");
        }

        product.Update(
            request.SKU,
            request.Name,
            request.Slug,
            request.Description,
            request.OriginalPrice,
            request.SalePrice,
            request.StockQuantity,
            request.CategoryId,
            request.Status);

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
