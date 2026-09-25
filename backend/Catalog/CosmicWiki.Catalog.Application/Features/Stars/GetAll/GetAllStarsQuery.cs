using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.GetAll;

public sealed record GetAllStarsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<StarListDto>>;

public sealed record StarListDto(
    Guid Id,
    string? Name,
    IReadOnlyCollection<CatalogIdentifierDto> CatalogIdentifiers);

public sealed record CatalogIdentifierDto(
    string Catalog,
    string Value);