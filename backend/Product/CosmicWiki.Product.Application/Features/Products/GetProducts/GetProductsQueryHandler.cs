using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Pagination;
using CosmicWiki.Caching;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Application.Features.Products.GetProducts;

public sealed class GetProductsQueryHandler
    : IRequestHandler<
        GetProductsQuery,
        PagedResult<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheStore _cache;
    private readonly IProductCache _productCache;
    private readonly CachingOptions _cachingOptions;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ICacheStore cache,
        IProductCache productCache,
        IOptions<CachingOptions> cachingOptions)
    {
        _productRepository = productRepository;
        _cache = cache;
        _productCache = productCache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<PagedResult<ProductResponse>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new ArgumentException(
                "Page must be greater than zero.",
                nameof(request.Page));

        if (request.PageSize < 1 ||
            request.PageSize > PaginationOptions.MaxPageSize)
        {
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(request.PageSize));
        }

        var cacheKey = await _productCache.GetListKeyAsync(
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);
        var cached = await _cache.GetAsync<PagedResult<ProductResponse>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var totalCount =
            await _productRepository.CountAsync(
                request.Search,
                cancellationToken);

        var products =
            await _productRepository.GetPageAsync(
                (request.Page - 1) * request.PageSize,
                request.PageSize,
                request.Search,
                cancellationToken);

        var items = products
            .Select(Map)
            .ToList();

        var result = new PagedResult<ProductResponse>(
            items,
            request.Page,
            request.PageSize,
            totalCount);

        var ttl = TimeSpan.FromSeconds(_cachingOptions.ProductListTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);
        return result;
    }

    private static ProductResponse Map(
        CosmicWiki.Product.Domain.Entities.Product product)
    {
        var images = product.Images
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ProductImageResponse(
                x.Id,
                x.Url,
                x.DisplayOrder))
            .ToList();

        return new ProductResponse(
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
    }
}