using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Product.Domain.Entities;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.CreateProductImage;

public sealed class CreateProductImageCommandHandler
    : IRequestHandler<CreateProductImageCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public CreateProductImageCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IProductCache productCache)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _productCache = productCache;
    }

    public async Task<Guid> Handle(
        CreateProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var product =
            await _productRepository.GetByIdForUpdateAsync(
                request.ProductId,
                cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product '{request.ProductId}' was not found.");
        }

        var image = ProductImage.Create(
            product.Id,
            request.Url,
            request.DisplayOrder);

        product.AddImage(image);

        ProductIndexingEvents.AddUpdated(_outboxWriter, product);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
        await _productCache.InvalidateAsync(
            product.Id,
            cancellationToken);

        return image.Id;
    }
}
