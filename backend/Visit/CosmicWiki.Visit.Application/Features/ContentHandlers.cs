using CosmicWiki.Pagination;
using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Application.Features.Events;
using CosmicWiki.Visit.Application.Features.Exhibitions;
using CosmicWiki.Visit.Application.Features.Faqs;
using CosmicWiki.Visit.Domain.Entities;
using MediatR;

namespace CosmicWiki.Visit.Application.Features.Exhibitions
{

public sealed class GetExhibitionsQueryHandler(IExhibitionRepository repository)
    : IRequestHandler<GetExhibitionsQuery, PagedResult<ExhibitionResponse>>
{
    public async Task<PagedResult<ExhibitionResponse>> Handle(
        GetExhibitionsQuery request,
        CancellationToken cancellationToken)
    {
        ValidatePage(request.Page, request.PageSize);
        var total = await repository.CountAsync(true, cancellationToken);
        var entities = await repository.GetPageAsync(
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            request.OrderBy,
            request.Descending,
            true,
            cancellationToken);
        return new PagedResult<ExhibitionResponse>(
            entities.Select(Map).ToArray(), request.Page, request.PageSize, total);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than zero.", nameof(page));
        if (pageSize is < 1 or > PaginationOptions.MaxPageSize)
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(pageSize));
    }

    internal static ExhibitionResponse Map(Exhibition entity) =>
        new(entity.Id, entity.Number, entity.Category, entity.Title, entity.Description,
            entity.Status, entity.ImageUrl, entity.DisplayOrder, entity.IsPublished);
}

public sealed class GetExhibitionByIdQueryHandler(IExhibitionRepository repository)
    : IRequestHandler<GetExhibitionByIdQuery, ExhibitionResponse?>
{
    public async Task<ExhibitionResponse?> Handle(
        GetExhibitionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, true, cancellationToken);
        return entity is null ? null : GetExhibitionsQueryHandler.Map(entity);
    }
}

public sealed class CreateExhibitionCommandHandler(
    IExhibitionRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<CreateExhibitionCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateExhibitionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = Exhibition.Create(
            request.Number, request.Category, request.Title, request.Description,
            request.Status, request.ImageUrl, request.DisplayOrder, request.IsPublished);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed class UpdateExhibitionCommandHandler(
    IExhibitionRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<UpdateExhibitionCommand>
{
    public async Task Handle(
        UpdateExhibitionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Exhibition '{request.Id}' was not found.");
        entity.Update(
            request.Number, request.Category, request.Title, request.Description,
            request.Status, request.ImageUrl, request.DisplayOrder, request.IsPublished);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteExhibitionCommandHandler(
    IExhibitionRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<DeleteExhibitionCommand>
{
    public async Task Handle(
        DeleteExhibitionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Exhibition '{request.Id}' was not found.");
        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
}

namespace CosmicWiki.Visit.Application.Features.Events
{

public sealed class GetEventsQueryHandler(IVisitEventRepository repository)
    : IRequestHandler<GetEventsQuery, PagedResult<EventResponse>>
{
    public async Task<PagedResult<EventResponse>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        ValidatePage(request.Page, request.PageSize);
        var total = await repository.CountAsync(true, cancellationToken);
        var entities = await repository.GetPageAsync(
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            request.OrderBy,
            request.Descending,
            true,
            cancellationToken);
        return new PagedResult<EventResponse>(
            entities.Select(Map).ToArray(), request.Page, request.PageSize, total);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than zero.", nameof(page));
        if (pageSize is < 1 or > PaginationOptions.MaxPageSize)
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(pageSize));
    }

    internal static EventResponse Map(VisitEvent entity) =>
        new(entity.Id, entity.EventDateUtc, entity.Name, entity.Description,
            entity.Specification, entity.Location, entity.Status, entity.StatusType,
            entity.Action, entity.DisplayOrder, entity.IsPublished);
}

public sealed class GetEventByIdQueryHandler(IVisitEventRepository repository)
    : IRequestHandler<GetEventByIdQuery, EventResponse?>
{
    public async Task<EventResponse?> Handle(
        GetEventByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, true, cancellationToken);
        return entity is null ? null : GetEventsQueryHandler.Map(entity);
    }
}

public sealed class CreateEventCommandHandler(
    IVisitEventRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<CreateEventCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateEventCommand request,
        CancellationToken cancellationToken)
    {
        var entity = VisitEvent.Create(
            request.EventDateUtc, request.Name, request.Description,
            request.Specification, request.Location, request.Status,
            request.StatusType, request.Action, request.DisplayOrder, request.IsPublished);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed class UpdateEventCommandHandler(
    IVisitEventRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEventCommand>
{
    public async Task Handle(
        UpdateEventCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Event '{request.Id}' was not found.");
        entity.Update(
            request.EventDateUtc, request.Name, request.Description,
            request.Specification, request.Location, request.Status,
            request.StatusType, request.Action, request.DisplayOrder, request.IsPublished);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteEventCommandHandler(
    IVisitEventRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(
        DeleteEventCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Event '{request.Id}' was not found.");
        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
}

namespace CosmicWiki.Visit.Application.Features.Faqs
{

public sealed class GetFaqsQueryHandler(IFaqRepository repository)
    : IRequestHandler<GetFaqsQuery, PagedResult<FaqResponse>>
{
    public async Task<PagedResult<FaqResponse>> Handle(
        GetFaqsQuery request,
        CancellationToken cancellationToken)
    {
        ValidatePage(request.Page, request.PageSize);
        var total = await repository.CountAsync(true, cancellationToken);
        var entities = await repository.GetPageAsync(
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            request.OrderBy,
            request.Descending,
            true,
            cancellationToken);
        return new PagedResult<FaqResponse>(
            entities.Select(Map).ToArray(), request.Page, request.PageSize, total);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than zero.", nameof(page));
        if (pageSize is < 1 or > PaginationOptions.MaxPageSize)
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(pageSize));
    }

    internal static FaqResponse Map(Faq entity) =>
        new(entity.Id, entity.Question, entity.Answer, entity.DisplayOrder, entity.IsPublished);
}

public sealed class GetFaqByIdQueryHandler(IFaqRepository repository)
    : IRequestHandler<GetFaqByIdQuery, FaqResponse?>
{
    public async Task<FaqResponse?> Handle(
        GetFaqByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, true, cancellationToken);
        return entity is null ? null : GetFaqsQueryHandler.Map(entity);
    }
}

public sealed class CreateFaqCommandHandler(
    IFaqRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<CreateFaqCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateFaqCommand request,
        CancellationToken cancellationToken)
    {
        var entity = Faq.Create(
            request.Question, request.Answer, request.DisplayOrder, request.IsPublished);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed class UpdateFaqCommandHandler(
    IFaqRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<UpdateFaqCommand>
{
    public async Task Handle(
        UpdateFaqCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"FAQ '{request.Id}' was not found.");
        entity.Update(request.Question, request.Answer, request.DisplayOrder, request.IsPublished);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteFaqCommandHandler(
    IFaqRepository repository,
    IVisitUnitOfWork unitOfWork)
    : IRequestHandler<DeleteFaqCommand>
{
    public async Task Handle(
        DeleteFaqCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"FAQ '{request.Id}' was not found.");
        repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    }
}
