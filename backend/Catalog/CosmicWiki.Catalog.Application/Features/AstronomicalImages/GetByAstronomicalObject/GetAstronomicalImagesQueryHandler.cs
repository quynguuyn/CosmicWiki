using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.GetByAstronomicalObject;

public sealed class GetAstronomicalImagesQueryHandler
    : IRequestHandler<
        GetAstronomicalImagesQuery,
        IReadOnlyCollection<AstronomicalImageDto>>
{
    private readonly IAstronomicalImageRepository _repository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetAstronomicalImagesQueryHandler(
        IAstronomicalImageRepository repository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _repository = repository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<IReadOnlyCollection<AstronomicalImageDto>> Handle(
        GetAstronomicalImagesQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"star:{request.AstronomicalObjectId:N}:images";
        
        var cached = await _cache.GetAsync<IReadOnlyCollection<AstronomicalImageDto>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var images =
            await _repository.GetByAstronomicalObjectIdAsync(
                request.AstronomicalObjectId,
                cancellationToken);

        var result = images
            .Select(image =>
                new AstronomicalImageDto(
                    image.Id,
                    image.AstronomicalObjectId,
                    image.Url,
                    image.Urls))
            .ToList();
            
        var ttl = TimeSpan.FromSeconds(_cachingOptions.CatalogListTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);
        
        return result;
    }
}