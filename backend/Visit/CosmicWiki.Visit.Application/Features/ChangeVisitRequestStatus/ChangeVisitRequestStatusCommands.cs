using CosmicWiki.Visit.Application.Abstractions;
using MediatR;

namespace CosmicWiki.Visit.Application.Features.ChangeVisitRequestStatus;

public sealed record ConfirmVisitRequestCommand(Guid Id) : IRequest;
public sealed record DeclineVisitRequestCommand(Guid Id) : IRequest;
public sealed record CancelVisitRequestCommand(Guid Id) : IRequest;

public abstract class ChangeVisitRequestStatusHandler(
    IVisitRequestRepository repository,
    IVisitUnitOfWork unitOfWork)
{
    protected async Task ChangeAsync(
        Guid id,
        Action<CosmicWiki.Visit.Domain.Entities.VisitRequest> transition,
        CancellationToken cancellationToken)
    {
        var request = await repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Visit request '{id}' was not found.");
        transition(request);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ConfirmVisitRequestCommandHandler(
    IVisitRequestRepository repository,
    IVisitUnitOfWork unitOfWork)
    : ChangeVisitRequestStatusHandler(repository, unitOfWork),
      IRequestHandler<ConfirmVisitRequestCommand>
{
    public Task Handle(ConfirmVisitRequestCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, request => request.Confirm(), cancellationToken);
}

public sealed class DeclineVisitRequestCommandHandler(
    IVisitRequestRepository repository,
    IVisitUnitOfWork unitOfWork)
    : ChangeVisitRequestStatusHandler(repository, unitOfWork),
      IRequestHandler<DeclineVisitRequestCommand>
{
    public Task Handle(DeclineVisitRequestCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, request => request.Decline(), cancellationToken);
}

public sealed class CancelVisitRequestCommandHandler(
    IVisitRequestRepository repository,
    IVisitUnitOfWork unitOfWork)
    : ChangeVisitRequestStatusHandler(repository, unitOfWork),
      IRequestHandler<CancelVisitRequestCommand>
{
    public Task Handle(CancelVisitRequestCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, request => request.Cancel(), cancellationToken);
}
