using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Product;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IProductCache _productCache;

    public DeleteProductCommandHandler(
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
        DeleteProductCommand request,
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

        _outboxWriter.Add(
            new ProductDeletedIntegrationEvent(
                Guid.NewGuid(), 1, DateTime.UtcNow, product.Id, product.Id),
            KafkaTopics.ProductDeleted);

        _productRepository.Remove(product);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
        await _productCache.InvalidateAsync(
            product.Id,
            cancellationToken);
    }
}
