using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Product.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ProductDbContext _dbContext;

    public CategoryRepository(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return await _dbContext.Categories
            .AnyAsync(
                x => x.Name == name,
                cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return await _dbContext.Categories
            .AnyAsync(
                x => x.Slug == slug,
                cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
    
    public async Task<Category?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetPageAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = Filter(search);
        return await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        CancellationToken cancellationToken = default) =>
        Filter(search).CountAsync(cancellationToken);

    private IQueryable<Category> Filter(string? search)
    {
        var query = _dbContext.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(category =>
                category.Name.ToLower().Contains(normalizedSearch) ||
                category.Slug.ToLower().Contains(normalizedSearch));
        }
        return query;
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        await _dbContext.Categories.AddAsync(
            category,
            cancellationToken);
    }

    public void Remove(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        _dbContext.Categories.Remove(category);
    }
}