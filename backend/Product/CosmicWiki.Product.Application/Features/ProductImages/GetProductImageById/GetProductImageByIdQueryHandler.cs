using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.ProductImages.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Application.Features.ProductImages.GetProductImageById;

public sealed class GetProductImageByIdQueryHandler
    : IRequestHandler<
        GetProductImageByIdQuery,
        ProductImageResponse?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetProductImageByIdQueryHandler(
        IProductRepository productRepository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _productRepository = productRepository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<ProductImageResponse?> Handle(
        GetProductImageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.ProductId:N}:image:{request.ImageId:N}";

        var cached = await _cache.GetAsync<ProductImageResponse>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var product =
            await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

        if (product is null)
            throw new KeyNotFoundException(
                $"Product '{request.ProductId}' was not found.");

        var image = product.Images
            .SingleOrDefault(x => x.Id == request.ImageId);

        if (image is null)
            return null;

        var result = new ProductImageResponse(
            image.Id,
            product.Id,
            image.Url,
            image.DisplayOrder);

        var ttl = TimeSpan.FromSeconds(_cachingOptions.ProductDetailTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);

        return result;
    }
}