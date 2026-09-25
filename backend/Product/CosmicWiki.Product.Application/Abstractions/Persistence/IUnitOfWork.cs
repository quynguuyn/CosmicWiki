namespace CosmicWiki.Product.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}