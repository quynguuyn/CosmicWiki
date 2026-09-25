using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.UpdateProductImage;

public sealed class UpdateProductImageCommandHandler
    : IRequestHandler<UpdateProductImageCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public UpdateProductImageCommandHandler(
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

    public async Task Handle(
        UpdateProductImageCommand request,
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

        var image = product.Images
            .SingleOrDefault(x => x.Id == request.ImageId);

        if (image is null)
        {
            throw new KeyNotFoundException(
                $"Product image '{request.ImageId}' was not found.");
        }

        product.UpdateImage(
            request.ImageId,
            request.Url,
            request.DisplayOrder);

        ProductIndexingEvents.AddUpdated(_outboxWriter, product);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
        await _productCache.InvalidateAsync(
            product.Id,
            cancellationToken);
    }
}
