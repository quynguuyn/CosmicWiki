using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Visit.Infrastructure.Persistence;

public sealed class VisitRequestRepository(VisitDbContext db) : IVisitRequestRepository
{
    public Task AddAsync(VisitRequest request, CancellationToken cancellationToken) =>
        db.VisitRequests.AddAsync(request, cancellationToken).AsTask();
    public Task<VisitRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.VisitRequests.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<VisitRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        db.VisitRequests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<VisitRequest?> GetByReferenceCodeAsync(string referenceCode, CancellationToken cancellationToken) =>
        db.VisitRequests.AsNoTracking().SingleOrDefaultAsync(x => x.ReferenceCode == referenceCode.Trim().ToUpper(), cancellationToken);
}
