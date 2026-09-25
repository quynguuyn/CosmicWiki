using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using MediatR;
namespace CosmicWiki.Visit.Application.Features.CreateVisitRequest;
public sealed class CreateVisitRequestCommandHandler(IVisitRequestRepository repository, IVisitUnitOfWork unitOfWork)
    : IRequestHandler<CreateVisitRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateVisitRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = VisitRequest.Create(request.Name, request.Email, request.RequestedDateUtc, request.PartySize, request.Notes);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
