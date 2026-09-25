using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Visit.Infrastructure.Persistence;

public sealed class ExhibitionRepository(VisitDbContext db) : IExhibitionRepository
{
    public Task<int> CountAsync(bool publishedOnly, CancellationToken cancellationToken) =>
        Query(publishedOnly).CountAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Exhibition>> GetPageAsync(
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
            "title" => descending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "category" => descending ? query.OrderByDescending(x => x.Category) : query.OrderBy(x => x.Category),
            _ => descending ? query.OrderByDescending(x => x.DisplayOrder) : query.OrderBy(x => x.DisplayOrder)
        };
        return await ordered.ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<Exhibition?> GetByIdAsync(Guid id, bool publishedOnly, CancellationToken cancellationToken) =>
        Query(publishedOnly).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Exhibition?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        db.Exhibitions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Exhibition exhibition, CancellationToken cancellationToken) =>
        db.Exhibitions.AddAsync(exhibition, cancellationToken).AsTask();

    public void Remove(Exhibition exhibition) => db.Exhibitions.Remove(exhibition);

    private IQueryable<Exhibition> Query(bool publishedOnly)
    {
        var query = db.Exhibitions.AsNoTracking();
        return publishedOnly ? query.Where(x => x.IsPublished) : query;
    }
}
