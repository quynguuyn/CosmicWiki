using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Visit.Infrastructure.Persistence;

public sealed class VisitEventRepository(VisitDbContext db) : IVisitEventRepository
{
    public Task<int> CountAsync(bool publishedOnly, CancellationToken cancellationToken) =>
        Query(publishedOnly).CountAsync(cancellationToken);

    public async Task<IReadOnlyCollection<VisitEvent>> GetPageAsync(
        int skip,
        int take,
        string orderBy,
        bool descending,
        bool publishedOnly,
        CancellationToken cancellationToken)
    {
        var query = Query(publishedOnly);
        var ordered = orderBy.Trim().ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "displayorder" => descending
                ? query.OrderByDescending(x => x.DisplayOrder)
                : query.OrderBy(x => x.DisplayOrder),
            _ => descending
                ? query.OrderByDescending(x => x.EventDateUtc)
                : query.OrderBy(x => x.EventDateUtc)
        };
        return await ordered.ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<VisitEvent?> GetByIdAsync(Guid id, bool publishedOnly, CancellationToken cancellationToken) =>
        Query(publishedOnly).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<VisitEvent?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        db.Events.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(VisitEvent visitEvent, CancellationToken cancellationToken) =>
        db.Events.AddAsync(visitEvent, cancellationToken).AsTask();

    public void Remove(VisitEvent visitEvent) => db.Events.Remove(visitEvent);

    private IQueryable<VisitEvent> Query(bool publishedOnly)
    {
        var query = db.Events.AsNoTracking();
        return publishedOnly ? query.Where(x => x.IsPublished) : query;
    }
}
