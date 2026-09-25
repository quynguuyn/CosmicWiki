using CosmicWiki.Catalog.Domain.Entities;

namespace CosmicWiki.Catalog.Application.Abstractions.Persistence;

public interface IAstronomicalObjectRepository
{
    Task<AstronomicalObject?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AstronomicalObject astronomicalObject,
        CancellationToken cancellationToken = default);
    
    void Delete(AstronomicalObject astronomicalObject);
}