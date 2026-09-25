using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Discovery.Application.Features.Search;

public sealed record SearchQuery(
    string? Search,
    string? SourceType,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SearchResultResponse>>;