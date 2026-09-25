using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.ProductImages.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Application.Features.ProductImages.GetProductImages;

public sealed class GetProductImagesQueryHandler
    : IRequestHandler<
        GetProductImagesQuery,
        IReadOnlyCollection<ProductImageResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetProductImagesQueryHandler(
        IProductRepository productRepository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _productRepository = productRepository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<IReadOnlyCollection<ProductImageResponse>> Handle(
        GetProductImagesQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.ProductId:N}:images";

        var cached = await _cache.GetAsync<IReadOnlyCollection<ProductImageResponse>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var product =
            await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product '{request.ProductId}' was not found.");
        }

        var result = product.Images
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ProductImageResponse(
                x.Id,
                product.Id,
                x.Url,
                x.DisplayOrder))
            .ToList();

        var ttl = TimeSpan.FromSeconds(_cachingOptions.ProductListTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);

        return result;
    }
}