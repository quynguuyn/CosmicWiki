using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Caching;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Application.Features.Products.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<
        GetProductByIdQuery,
        ProductResponse?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _productRepository = productRepository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<ProductResponse?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.Id:N}";
        var cached = await _cache.GetAsync<ProductResponse>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var product =
            await _productRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (product is null)
            return null;

        var images = product.Images
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ProductImageResponse(
                x.Id,
                x.Url,
                x.DisplayOrder))
            .ToList();

        var result = new ProductResponse(
            product.Id,
            product.SKU,
            product.Name,
            product.Slug,
            product.Description,
            product.OriginalPrice,
            product.SalePrice,
            product.StockQuantity,
            product.Status,
            product.CategoryId,
            images,
            product.Images
                .OrderBy(x => x.DisplayOrder)
                .SelectMany(x => x.Urls)
                .ToList());

        var ttl = TimeSpan.FromSeconds(_cachingOptions.ProductDetailTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);
        return result;
    }
}