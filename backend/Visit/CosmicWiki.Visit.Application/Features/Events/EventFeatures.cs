using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Visit.Application.Features.Events;

public sealed record EventResponse(
    Guid Id,
    DateTime EventDateUtc,
    string Name,
    string Description,
    string? Specification,
    string? Location,
    string Status,
    string? StatusType,
    string? Action,
    int DisplayOrder,
    bool IsPublished);

public sealed record GetEventsQuery(
    int Page = 1,
    int PageSize = 20,
    string OrderBy = "eventDateUtc",
    bool Descending = false) : IRequest<PagedResult<EventResponse>>;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventResponse?>;

public sealed record CreateEventCommand(
    DateTime EventDateUtc,
    string Name,
    string Description,
    string? Specification,
    string? Location,
    string Status,
    string? StatusType,
    string? Action,
    int DisplayOrder = 0,
    bool IsPublished = true) : IRequest<Guid>;

public sealed record UpdateEventCommand(
    Guid Id,
    DateTime EventDateUtc,
    string Name,
    string Description,
    string? Specification,
    string? Location,
    string Status,
    string? StatusType,
    string? Action,
    int DisplayOrder,
    bool IsPublished) : IRequest;

public sealed record DeleteEventCommand(Guid Id) : IRequest;
