using CosmicWiki.Catalog.Application.Features.Stars.GetAll;
using CosmicWiki.Catalog.Domain.Entities;

namespace CosmicWiki.Catalog.Application.Abstractions.Persistence;

public interface IStarRepository
{
    Task<Star?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Star?> GetByCatalogIdentifierAsync(
        string catalog,
        string value,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetExistingCatalogIdentifierValuesAsync(
        string catalog,
        IReadOnlyCollection<string> values,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Star star,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<StarListReadModel> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int pageSize,
            CancellationToken cancellationToken = default);
    
    void Delete(Star star);
}