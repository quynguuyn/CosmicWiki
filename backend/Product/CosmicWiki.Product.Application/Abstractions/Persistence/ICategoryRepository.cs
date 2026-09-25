using CosmicWiki.Product.Domain.Entities;

namespace CosmicWiki.Product.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Category>> GetAllAsync(
        CancellationToken cancellationToken = default);
    
    Task<Category?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);

    void Remove(Category category);
}