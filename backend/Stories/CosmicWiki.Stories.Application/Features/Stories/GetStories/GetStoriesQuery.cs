using CosmicWiki.Pagination;
using CosmicWiki.Stories.Application.Features.Stories.Common;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.GetStories;

public sealed record GetStoriesQuery(
    string? Search = null,
    string? Category = null,
    int Page = 1,
    int PageSize = 20,
    string Sort = "newest") : IRequest<PagedResult<StoryResponse>>;
