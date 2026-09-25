using CosmicWiki.Visit.Domain.Entities;

namespace CosmicWiki.Visit.Application.Abstractions;

public interface IExhibitionRepository
{
    Task<int> CountAsync(bool publishedOnly, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Exhibition>> GetPageAsync(
        int skip,
        int take,
        string orderBy,
        bool descending,
        bool publishedOnly,
        CancellationToken cancellationToken);
    Task<Exhibition?> GetByIdAsync(Guid id, bool publishedOnly, CancellationToken cancellationToken);
    Task<Exhibition?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Exhibition exhibition, CancellationToken cancellationToken);
    void Remove(Exhibition exhibition);
}
