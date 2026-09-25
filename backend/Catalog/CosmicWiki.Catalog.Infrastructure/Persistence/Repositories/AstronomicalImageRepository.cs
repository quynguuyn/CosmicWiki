using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Repositories;

public sealed class AstronomicalImageRepository
    : IAstronomicalImageRepository
{
    private readonly CatalogDbContext _dbContext;

    public AstronomicalImageRepository(
        CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        AstronomicalImage image,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        await _dbContext.AstronomicalImages.AddAsync(
            image,
            cancellationToken);
    }

    public async Task<IReadOnlyList<AstronomicalImage>>
        GetByAstronomicalObjectIdAsync(
            Guid astronomicalObjectId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.AstronomicalImages
            .Where(x =>
                x.AstronomicalObjectId == astronomicalObjectId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<AstronomicalImage?> GetByIdAsync(
        Guid astronomicalObjectId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AstronomicalImages
            .SingleOrDefaultAsync(
                x =>
                    x.Id == imageId &&
                    x.AstronomicalObjectId == astronomicalObjectId,
                cancellationToken);
    }
    
    public void Delete(AstronomicalImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        _dbContext.AstronomicalImages.Remove(image);
    }
}