using CosmicWiki.Visit.Domain.Entities;
namespace CosmicWiki.Visit.Application.Abstractions;
public interface IVisitRequestRepository
{
    Task AddAsync(VisitRequest request, CancellationToken cancellationToken);
    Task<VisitRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<VisitRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<VisitRequest?> GetByReferenceCodeAsync(string referenceCode, CancellationToken cancellationToken);
}
