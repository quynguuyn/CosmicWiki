using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<
        GetCategoriesQuery,
        IReadOnlyCollection<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheStore _cache;
    private readonly ICategoryCache _categoryCache;
    private readonly CachingOptions _cachingOptions;

    public GetCategoriesQueryHandler(
        ICategoryRepository categoryRepository,
        ICacheStore cache,
        ICategoryCache categoryCache,
        IOptions<CachingOptions> cachingOptions)
    {
        _categoryRepository = categoryRepository;
        _cache = cache;
        _categoryCache = categoryCache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<IReadOnlyCollection<CategoryResponse>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = await _categoryCache.GetListKeyAsync(cancellationToken);

        var cached = await _cache.GetAsync<IReadOnlyCollection<CategoryResponse>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var categories =
            await _categoryRepository.GetAllAsync(
                cancellationToken);

        var result = categories
            .Select(x => new CategoryResponse(
                x.Id,
                x.Name,
                x.Slug,
                x.Description))
            .ToArray();

        var ttl = TimeSpan.FromSeconds(_cachingOptions.ProductListTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);

        return result;
    }
}