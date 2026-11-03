using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Features.Products.Common;
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

        await ProductUpdateWorkflow.ApplyAsync(
            product,
            request.SKU,
            request.Name,
            request.Slug,
            request.Description,
            request.OriginalPrice,
            request.SalePrice,
            request.StockQuantity,
            request.CategoryId,
            request.Status,
            validateCategory: true,
            _productRepository,
            _categoryRepository,
            _unitOfWork,
            _outboxWriter,
            _productCache,
            cancellationToken);
    }
}
