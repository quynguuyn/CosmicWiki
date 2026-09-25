using CosmicWiki.Caching;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Application.Features.Stories.Common;
using CosmicWiki.Stories.Application.Features.Stories.GetStories;
using MediatR;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Stories.Application.Features.Stories.GetStoryById;

public sealed class GetStoryByIdQueryHandler : IRequestHandler<GetStoryByIdQuery, StoryResponse?>
{
    private readonly IStoryRepository _repository;
    private readonly ICacheStore _cache;
    private readonly IStoryCache _storyCache;
    private readonly CachingOptions _options;

    public GetStoryByIdQueryHandler(
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

    public async Task<StoryResponse?> Handle(GetStoryByIdQuery request, CancellationToken cancellationToken)
    {
        var key = _storyCache.GetDetailKey(request.Id);
        var cached = await _cache.GetAsync<StoryResponse>(key, cancellationToken);
        if (cached is not null)
            return cached;
        var story = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (story is null)
            return null;
        var result = GetStoriesQueryHandler.Map(story);
        await _cache.SetAsync(key, result, TimeSpan.FromSeconds(_options.StoryDetailTtlSeconds), cancellationToken);
        return result;
    }
}
