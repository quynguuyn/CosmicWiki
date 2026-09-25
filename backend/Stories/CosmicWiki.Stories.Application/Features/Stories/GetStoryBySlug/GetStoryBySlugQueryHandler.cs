using CosmicWiki.Caching;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Application.Features.Stories.Common;
using CosmicWiki.Stories.Application.Features.Stories.GetStories;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Stories.Application.Features.Stories.GetStoryBySlug;

public sealed class GetStoryBySlugQueryHandler : IRequestHandler<GetStoryBySlugQuery, StoryResponse?>
{
    private readonly IStoryRepository _repository;
    private readonly ICacheStore _cache;
    private readonly IStoryCache _storyCache;
    private readonly CachingOptions _options;

    public GetStoryBySlugQueryHandler(
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

    public async Task<StoryResponse?> Handle(GetStoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var key = _storyCache.GetSlugKey(request.Slug);
        var cached = await _cache.GetAsync<StoryResponse>(key, cancellationToken);
        if (cached is not null)
            return cached;
        var story = await _repository.GetBySlugAsync(request.Slug, cancellationToken);
        if (story is null)
            return null;
        var result = GetStoriesQueryHandler.Map(story);
        await _cache.SetAsync(key, result, TimeSpan.FromSeconds(_options.StoryDetailTtlSeconds), cancellationToken);
        return result;
    }
}
