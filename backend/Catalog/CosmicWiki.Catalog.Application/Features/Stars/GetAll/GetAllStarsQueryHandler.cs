using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Pagination;
using CosmicWiki.Caching;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Catalog.Application.Features.Stars.GetAll;

public sealed class GetAllStarsQueryHandler
    : IRequestHandler<GetAllStarsQuery, PagedResult<StarListDto>>
{
    private readonly IStarRepository _starRepository;
    private readonly ICacheStore _cache;
    private readonly IStarCache _starCache;
    private readonly CachingOptions _cachingOptions;

    public GetAllStarsQueryHandler(
        IStarRepository starRepository,
        ICacheStore cache,
        IStarCache starCache,
        IOptions<CachingOptions> cachingOptions)
    {
        _starRepository = starRepository;
        _cache = cache;
        _starCache = starCache;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<PagedResult<StarListDto>> Handle(
        GetAllStarsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new ArgumentOutOfRangeException(
                nameof(request.Page),
                "Page must be greater than or equal to 1.");

        if (request.PageSize < 1)
            throw new ArgumentOutOfRangeException(
                nameof(request.PageSize),
                "Page size must be greater than 0.");

        if (request.PageSize > PaginationOptions.MaxPageSize)
            throw new ArgumentOutOfRangeException(
                nameof(request.PageSize),
                $"Page size cannot exceed {PaginationOptions.MaxPageSize}.");

        var options = new PaginationOptions(
            request.Page,
            request.PageSize);

        var cacheKey = await _starCache.GetListKeyAsync(
            options.Page,
            options.PageSize,
            cancellationToken);

        var cached = await _cache.GetAsync<PagedResult<StarListDto>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
            return cached;

        var result = await _starRepository.GetPagedAsync(
            options.Skip,
            options.PageSize,
            cancellationToken);

        var items = result.Items
            .Select(x => new StarListDto(
                x.Id,
                x.Name,
                x.CatalogIdentifiers
                    .Select(identifier =>
                        new CatalogIdentifierDto(
                            identifier.Catalog,
                            identifier.Value))
                    .ToList()))
            .ToList();

        var pagedResult = new PagedResult<StarListDto>(
            items,
            options.Page,
            options.PageSize,
            result.TotalCount);

        var ttl = TimeSpan.FromSeconds(_cachingOptions.CatalogListTtlSeconds);
        await _cache.SetAsync(cacheKey, pagedResult, ttl, cancellationToken);
        
        return pagedResult;
    }
}