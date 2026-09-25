using CosmicWiki.Caching;
using CosmicWiki.Pagination;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Application.Features.Stories.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Stories.Application.Features.Stories.GetStories;

public sealed class GetStoriesQueryHandler : IRequestHandler<GetStoriesQuery, PagedResult<StoryResponse>>
{
    private readonly IStoryRepository _repository;
    private readonly ICacheStore _cache;
    private readonly IStoryCache _storyCache;
    private readonly CachingOptions _options;

    public GetStoriesQueryHandler(
        IStoryRepository repository,
        ICacheStore cache,
        IStoryCache storyCache,
        IOptions<CachingOptions> options)
    {
        _repository = repository;
        _cache = cache;
        _storyCache = storyCache;
        _options = options.Value;
    }

    public async Task<PagedResult<StoryResponse>> Handle(
        GetStoriesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new ArgumentException("Page must be greater than zero.", nameof(request.Page));
        if (request.PageSize < 1 || request.PageSize > PaginationOptions.MaxPageSize)
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(request.PageSize));

        var sort = NormalizeSort(request.Sort);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        var category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        var cacheKey = await _storyCache.GetListKeyAsync(
            search, category, request.Page, request.PageSize, sort, cancellationToken);
        var cached = await _cache.GetAsync<PagedResult<StoryResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var totalCount = await _repository.CountAsync(search, category, cancellationToken);
        var stories = await _repository.GetPageAsync(
            search, category, (request.Page - 1) * request.PageSize, request.PageSize, sort, cancellationToken);
        var result = new PagedResult<StoryResponse>(
            stories.Select(Map).ToList(), request.Page, request.PageSize, totalCount);
        await _cache.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromSeconds(_options.StoryListTtlSeconds),
            cancellationToken);
        return result;
    }

    internal static string NormalizeSort(string? sort) =>
        sort?.Trim().ToLowerInvariant() switch
        {
            "oldest" => "oldest",
            "title" => "title",
            "duration" or "readtime" => "duration",
            "popular" => "popular",
            _ => "newest"
        };

    internal static StoryResponse Map(CosmicWiki.Stories.Domain.Entities.Story story) =>
        new(
            story.Id,
            story.Category,
            story.Era,
            story.Code,
            story.Title,
            story.Slug,
            story.Description,
            story.Content,
            story.Author,
            story.ReadTimeMinutes,
            story.ImageUrl,
            story.ImageAlt,
            story.Status,
            story.CreatedAtUtc,
            story.UpdatedAtUtc);
}
