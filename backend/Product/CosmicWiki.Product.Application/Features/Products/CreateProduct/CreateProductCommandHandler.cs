using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Product;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Domain.Entities;
using CosmicWiki.Caching;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;

using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public CreateProductCommandHandler(
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

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (await _productRepository.ExistsBySkuAsync(
                request.SKU,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Product SKU '{request.SKU}' already exists.");
        }

        if (await _productRepository.ExistsBySlugAsync(
                request.Slug,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Product slug '{request.Slug}' already exists.");
        }

        if (!await _categoryRepository.ExistsAsync(
                request.CategoryId,
                cancellationToken))
        {
            throw new KeyNotFoundException(
                $"Category '{request.CategoryId}' was not found.");
        }

        var product = ProductEntity.Create(
            request.SKU,
            request.Name,
            request.Slug,
            request.OriginalPrice,
            request.SalePrice,
            request.StockQuantity,
            request.CategoryId,
            request.Description,
            request.Status);

        if (request.Images is not null)
        {
            foreach (var image in request.Images)
            {
                product.AddImage(
                    ProductImage.Create(
                        product.Id,
                        image.Url,
                        image.DisplayOrder));
            }
        }

        await _productRepository.AddAsync(
            product,
            cancellationToken);
        
        var integrationEvent =
            new ProductCreatedIntegrationEvent(
                Guid.NewGuid(),
                1,
                DateTime.UtcNow,
                product.Id,
                product.Id,
                product.SKU,
                product.Name,
                product.Slug,
                product.Description,
                product.Images
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => x.Url)
                    .FirstOrDefault());
        
        _outboxWriter.Add(
            integrationEvent,
            KafkaTopics.ProductCreated);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
        await _productCache.InvalidateAsync(
            product.Id,
            cancellationToken);
        
        return product.Id;
    }
}