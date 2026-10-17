using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
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

    public async Task<AstronomicalObjectReadModel?> GetReadModelByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AstronomicalObjects
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new AstronomicalObjectReadModel(
                item.Id,
                item.Name,
                item.Kind,
                item.ObjectType,
                item.Catalog,
                item.CatalogValue,
                item.RightAscensionDegrees,
                item.DeclinationDegrees,
                _dbContext.AstronomicalImages
                    .Where(image => image.AstronomicalObjectId == item.Id)
                    .OrderBy(image => image.Id)
                    .Select(image => image.Url)
                    .FirstOrDefault()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AstronomicalObjectImageTarget>>
        GetImageTargetsByCatalogValuesAsync(
            string catalog,
            IReadOnlyCollection<string> catalogValues,
            CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalog);
        ArgumentNullException.ThrowIfNull(catalogValues);
        if (catalogValues.Count == 0)
            return [];

        return await _dbContext.AstronomicalObjects
            .AsNoTracking()
            .Where(item =>
                item.Catalog == catalog &&
                item.CatalogValue != null &&
                catalogValues.Contains(item.CatalogValue))
            .Select(item => new AstronomicalObjectImageTarget(
                item.Id,
                item.Kind,
                item.CatalogValue,
                item.RightAscensionDegrees,
                item.DeclinationDegrees))
            .ToListAsync(cancellationToken);
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

    public async Task<(IReadOnlyList<AstronomicalObjectReadModel> Items, int TotalCount)>
        GetPagedAsync(
            int skip,
            int pageSize,
            AstronomicalObjectKind? kind = null,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AstronomicalObjects.AsNoTracking();
        if (kind.HasValue)
            query = query.Where(item => item.Kind == kind.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(item => new AstronomicalObjectReadModel(
                item.Id,
                item.Name,
                item.Kind,
                item.ObjectType,
                item.Catalog,
                item.CatalogValue,
                item.RightAscensionDegrees,
                item.DeclinationDegrees,
                _dbContext.AstronomicalImages
                    .Where(image => image.AstronomicalObjectId == item.Id)
                    .OrderBy(image => image.Id)
                    .Select(image => image.Url)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    
    public void Delete(AstronomicalObject astronomicalObject)
    {
        ArgumentNullException.ThrowIfNull(astronomicalObject);

        _dbContext.AstronomicalObjects.Remove(astronomicalObject);
    }
}