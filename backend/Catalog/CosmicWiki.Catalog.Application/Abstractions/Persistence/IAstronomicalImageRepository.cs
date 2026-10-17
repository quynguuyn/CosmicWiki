using CosmicWiki.Catalog.Domain.Entities;

namespace CosmicWiki.Catalog.Application.Abstractions.Persistence;

public interface IAstronomicalImageRepository
{
    Task AddAsync(
        AstronomicalImage image,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AstronomicalImage>> GetByAstronomicalObjectIdAsync(
        Guid astronomicalObjectId,
        CancellationToken cancellationToken = default);

    Task<bool> HasImageForAstronomicalObjectAsync(
        Guid astronomicalObjectId,
        CancellationToken cancellationToken = default);
    
    Task<AstronomicalImage?> GetByIdAsync(
        Guid astronomicalObjectId,
        Guid imageId,
        CancellationToken cancellationToken = default);

    void Delete(AstronomicalImage image);
}