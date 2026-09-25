using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Repositories;

public sealed class AstronomicalObjectRepository
    : IAstronomicalObjectRepository
{
    private readonly CatalogDbContext _dbContext;

    public AstronomicalObjectRepository(
        CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<AstronomicalObject?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AstronomicalObjects
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        AstronomicalObject astronomicalObject,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(astronomicalObject);

        await _dbContext.AstronomicalObjects.AddAsync(
            astronomicalObject,
            cancellationToken);
    }
    
    public void Delete(AstronomicalObject astronomicalObject)
    {
        ArgumentNullException.ThrowIfNull(astronomicalObject);

        _dbContext.AstronomicalObjects.Remove(astronomicalObject);
    }
}