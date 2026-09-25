using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Visit.Application.Features.Exhibitions;

public sealed record ExhibitionResponse(
    Guid Id,
    string Number,
    string Category,
    string Title,
    string Description,
    string Status,
    string? ImageUrl,
    int DisplayOrder,
    bool IsPublished);

public sealed record GetExhibitionsQuery(
    int Page = 1,
    int PageSize = 20,
    string OrderBy = "displayOrder",
    bool Descending = false) : IRequest<PagedResult<ExhibitionResponse>>;

public sealed record GetExhibitionByIdQuery(Guid Id) : IRequest<ExhibitionResponse?>;

public sealed record CreateExhibitionCommand(
    string Number,
    string Category,
    string Title,
    string Description,
    string Status,
    string? ImageUrl,
    int DisplayOrder = 0,
    bool IsPublished = true) : IRequest<Guid>;

public sealed record UpdateExhibitionCommand(
    Guid Id,
    string Number,
    string Category,
    string Title,
    string Description,
    string Status,
    string? ImageUrl,
    int DisplayOrder,
    bool IsPublished) : IRequest;

public sealed record DeleteExhibitionCommand(Guid Id) : IRequest;
