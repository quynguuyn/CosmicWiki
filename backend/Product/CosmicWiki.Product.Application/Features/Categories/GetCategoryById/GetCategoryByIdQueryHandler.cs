using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Application.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<
        GetCategoryByIdQuery,
        CategoryResponse?>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetCategoryByIdQueryHandler(
        ICategoryRepository categoryRepository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _categoryRepository = categoryRepository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<CategoryResponse?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"category:{request.Id:N}";

        var cached = await _cache.GetAsync<CategoryResponse>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var category =
            await _categoryRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (category is null)
            return null;

        var result = new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.Description);

        var ttl = TimeSpan.FromSeconds(_cachingOptions.ProductDetailTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);

        return result;
    }
}