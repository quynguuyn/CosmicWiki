using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Visit.Application.Features.Faqs;

public sealed record FaqResponse(
    Guid Id,
    string Question,
    string Answer,
    int DisplayOrder,
    bool IsPublished);

public sealed record GetFaqsQuery(
    int Page = 1,
    int PageSize = 20,
    string OrderBy = "displayOrder",
    bool Descending = false) : IRequest<PagedResult<FaqResponse>>;

public sealed record GetFaqByIdQuery(Guid Id) : IRequest<FaqResponse?>;

public sealed record CreateFaqCommand(
    string Question,
    string Answer,
    int DisplayOrder = 0,
    bool IsPublished = true) : IRequest<Guid>;

public sealed record UpdateFaqCommand(
    Guid Id,
    string Question,
    string Answer,
    int DisplayOrder,
    bool IsPublished) : IRequest;

public sealed record DeleteFaqCommand(Guid Id) : IRequest;
