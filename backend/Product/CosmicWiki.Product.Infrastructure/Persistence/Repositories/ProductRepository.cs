using CosmicWiki.Product.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;

namespace CosmicWiki.Product.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _dbContext;

    public ProductRepository(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Images)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<ProductEntity?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.Images)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Images)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductEntity>> GetPageAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken = default,
        Guid? categoryId = null)
    {
        var query = _dbContext.Products.AsNoTracking();
        if (categoryId.HasValue)
            query = query.Where(product => product.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(product =>
                product.SKU.ToLower().Contains(normalizedSearch) ||
                product.Name.ToLower().Contains(normalizedSearch) ||
                product.Slug.ToLower().Contains(normalizedSearch));
        }

        return await query
            .Include(x => x.Images)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? search,
        CancellationToken cancellationToken = default,
        Guid? categoryId = null)
    {
        var query = _dbContext.Products.AsNoTracking();
        if (categoryId.HasValue)
            query = query.Where(product => product.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(product =>
                product.SKU.ToLower().Contains(normalizedSearch) ||
                product.Name.ToLower().Contains(normalizedSearch) ||
                product.Slug.ToLower().Contains(normalizedSearch));
        }
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AnyAsync(
                x => x.SKU == sku,
                cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AnyAsync(
                x => x.Slug == slug,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        ProductEntity product,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(
            product,
            cancellationToken);
    }

    public void Remove(ProductEntity product)
    {
        _dbContext.Products.Remove(product);
    }
}