using CosmicWiki.Product.Application.Abstractions.Persistence;

namespace CosmicWiki.Product.Infrastructure.Persistence.Repositories;

public sealed class ProductUnitOfWork
    : IUnitOfWork
{
    private readonly ProductDbContext _dbContext;

    public ProductUnitOfWork(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}