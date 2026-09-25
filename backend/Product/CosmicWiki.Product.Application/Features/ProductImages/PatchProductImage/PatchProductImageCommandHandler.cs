using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.PatchProductImage;

public sealed class PatchProductImageCommandHandler
    : IRequestHandler<PatchProductImageCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public PatchProductImageCommandHandler(
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
        PatchProductImageCommand request,
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

        var url = request.Url ?? image.Url;
        var displayOrder =
            request.DisplayOrder ?? image.DisplayOrder;

        image.Update(
            url,
            displayOrder);

        ProductIndexingEvents.AddUpdated(_outboxWriter, product);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
        await _productCache.InvalidateAsync(
            product.Id,
            cancellationToken);
    }
}
