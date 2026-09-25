using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using MediatR;
namespace CosmicWiki.Visit.Application.Features.GetVisitRequest;
public sealed class GetVisitRequestQueryHandler(IVisitRequestRepository repository) : IRequestHandler<GetVisitRequestQuery, VisitRequest?>
{
    public Task<VisitRequest?> Handle(GetVisitRequestQuery request, CancellationToken cancellationToken) =>
        request.Id.HasValue ? repository.GetByIdAsync(request.Id.Value, cancellationToken) :
        repository.GetByReferenceCodeAsync(request.ReferenceCode!, cancellationToken);
}
