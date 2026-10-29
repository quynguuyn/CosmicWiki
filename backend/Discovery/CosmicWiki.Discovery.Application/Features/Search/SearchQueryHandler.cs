using CosmicWiki.Discovery.Application.Abstractions.Persistence;
using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Discovery.Application.Features.Search;

public sealed class SearchQueryHandler
    : IRequestHandler<
        SearchQuery,
        PagedResult<SearchResultResponse>>
{
    private readonly ISearchDocumentRepository _repository;

    public SearchQueryHandler(
        ISearchDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<SearchResultResponse>> Handle(
        SearchQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Page));
        }

        if (request.PageSize < 1 ||
            request.PageSize > PaginationOptions.MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.PageSize));
        }

        var totalCount =
            await _repository.CountAsync(
                request.Search,
                request.SourceType,
                cancellationToken);

        var documents =
            await _repository.SearchAsync(
                request.Search,
                request.SourceType,
                (request.Page - 1) * request.PageSize,
                request.PageSize,
                cancellationToken);

        var items = documents
            .Select(x =>
                new SearchResultResponse(
                    x.Id,
                    x.SourceId,
                    x.SourceType,
                    x.Name,
                    x.Description,
                    x.ImageUrl,
                    x.Catalog,
                    x.CatalogValue))
            .ToList();

        return new PagedResult<SearchResultResponse>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}