using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Application.Features.Stars.GetAll;
using CosmicWiki.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Repositories;

public sealed class StarRepository : IStarRepository
{
    private readonly CatalogDbContext _dbContext;

    public StarRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Star?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stars
            .Include(x => x.CatalogIdentifiers)
            .Include(x => x.Parallaxes)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Star?> GetByCatalogIdentifierAsync(
        string catalog,
        string value,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(catalog))
            throw new ArgumentException(
                "Catalog cannot be empty.",
                nameof(catalog));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Identifier value cannot be empty.",
                nameof(value));

        return await _dbContext.Stars
            .SingleOrDefaultAsync(
                star => star.CatalogIdentifiers.Any(
                    identifier =>
                        identifier.Catalog == catalog &&
                        identifier.Value == value),
                cancellationToken);
    }

    public async Task AddAsync(
        Star star,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(star);

        await _dbContext.Stars.AddAsync(
            star,
            cancellationToken);
    }
    
    public async Task<(IReadOnlyList<StarListReadModel> Items, int TotalCount)>
        GetPagedAsync(
            int skip,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbContext.Stars
            .CountAsync(cancellationToken);

        var pageStarIds = await _dbContext.Stars
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (pageStarIds.Count == 0)
        {
            return ([], totalCount);
        }

        var stars = await _dbContext.Stars
            .AsNoTracking()
            .Include(x => x.CatalogIdentifiers)
            .Where(x => pageStarIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var items = pageStarIds
            .Select(id =>
                stars.Single(star => star.Id == id))
            .Select(star =>
                new StarListReadModel(
                    star.Id,
                    star.Name,
                    star.CatalogIdentifiers
                        .Select(identifier =>
                            new CatalogIdentifierReadModel(
                                identifier.Catalog,
                                identifier.Value))
                        .ToList()))
            .ToList();

        return (items, totalCount);
    }
    
    public void Delete(Star star)
    {
        ArgumentNullException.ThrowIfNull(star);

        _dbContext.Stars.Remove(star);
    }
}