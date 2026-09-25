namespace CosmicWiki.Catalog.Application.Abstractions.Persistence;

public interface ICatalogUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}