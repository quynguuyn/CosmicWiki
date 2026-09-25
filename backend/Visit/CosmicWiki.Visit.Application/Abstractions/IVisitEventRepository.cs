using CosmicWiki.Visit.Domain.Entities;

namespace CosmicWiki.Visit.Application.Abstractions;

public interface IVisitEventRepository
{
    Task<int> CountAsync(bool publishedOnly, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VisitEvent>> GetPageAsync(
        int skip,
        int take,
        string orderBy,
        bool descending,
        bool publishedOnly,
        CancellationToken cancellationToken);
    Task<VisitEvent?> GetByIdAsync(Guid id, bool publishedOnly, CancellationToken cancellationToken);
    Task<VisitEvent?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(VisitEvent visitEvent, CancellationToken cancellationToken);
    void Remove(VisitEvent visitEvent);
}
