using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Visit.Infrastructure.Persistence;

public sealed class FaqRepository(VisitDbContext db) : IFaqRepository
{
    public Task<int> CountAsync(bool publishedOnly, CancellationToken cancellationToken) =>
        Query(publishedOnly).CountAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Faq>> GetPageAsync(
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
            "question" => descending ? query.OrderByDescending(x => x.Question) : query.OrderBy(x => x.Question),
            _ => descending ? query.OrderByDescending(x => x.DisplayOrder) : query.OrderBy(x => x.DisplayOrder)
        };
        return await ordered.ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<Faq?> GetByIdAsync(Guid id, bool publishedOnly, CancellationToken cancellationToken) =>
        Query(publishedOnly).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Faq?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        db.Faqs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Faq faq, CancellationToken cancellationToken) =>
        db.Faqs.AddAsync(faq, cancellationToken).AsTask();

    public void Remove(Faq faq) => db.Faqs.Remove(faq);

    private IQueryable<Faq> Query(bool publishedOnly)
    {
        var query = db.Faqs.AsNoTracking();
        return publishedOnly ? query.Where(x => x.IsPublished) : query;
    }
}
