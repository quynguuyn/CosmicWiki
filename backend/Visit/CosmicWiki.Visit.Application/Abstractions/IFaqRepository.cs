using CosmicWiki.Visit.Domain.Entities;

namespace CosmicWiki.Visit.Application.Abstractions;

public interface IFaqRepository
{
    Task<int> CountAsync(bool publishedOnly, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Faq>> GetPageAsync(
        int skip,
        int take,
        string orderBy,
        bool descending,
        bool publishedOnly,
        CancellationToken cancellationToken);
    Task<Faq?> GetByIdAsync(Guid id, bool publishedOnly, CancellationToken cancellationToken);
    Task<Faq?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Faq faq, CancellationToken cancellationToken);
    void Remove(Faq faq);
}
