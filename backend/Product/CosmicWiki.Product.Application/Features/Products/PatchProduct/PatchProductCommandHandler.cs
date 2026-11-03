using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Features.Products.Common;
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

        await ProductUpdateWorkflow.ApplyAsync(
            product,
            sku,
            name,
            slug,
            description,
            originalPrice,
            salePrice,
            stockQuantity,
            categoryId,
            status,
            validateCategory: categoryId != product.CategoryId,
            _productRepository,
            _categoryRepository,
            _unitOfWork,
            _outboxWriter,
            _productCache,
            cancellationToken);
    }
}
