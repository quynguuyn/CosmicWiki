using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Caching;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.GetById;

public sealed class GetAstronomicalImageByIdQueryHandler
    : IRequestHandler<
        GetAstronomicalImageByIdQuery,
        AstronomicalImageDto?>
{
    private readonly IAstronomicalImageRepository _imageRepository;
    private readonly ICacheStore _cache;
    private readonly CachingOptions _cachingOptions;

    public GetAstronomicalImageByIdQueryHandler(
        IAstronomicalImageRepository imageRepository,
        ICacheStore cache,
        IOptions<CachingOptions> cachingOptions)
    {
        _imageRepository = imageRepository;
        _cache = cache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<AstronomicalImageDto?> Handle(
        GetAstronomicalImageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"image:{request.ImageId:N}";
        
        var cached = await _cache.GetAsync<AstronomicalImageDto>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var image = await _imageRepository.GetByIdAsync(
            request.AstronomicalObjectId,
            request.ImageId,
            cancellationToken);

        if (image is null)
            return null;

        var result = new AstronomicalImageDto(
            image.Id,
            image.AstronomicalObjectId,
            image.Url,
            image.Urls.ToList());
            
        var ttl = TimeSpan.FromSeconds(_cachingOptions.CatalogDetailTtlSeconds);
        await _cache.SetAsync(cacheKey, result, ttl, cancellationToken);
        
        return result;
    }
}