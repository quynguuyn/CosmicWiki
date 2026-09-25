using CosmicWiki.Stories.Domain.Entities;

namespace CosmicWiki.Stories.Application.Abstractions.Persistence;

public interface IStoryRepository
{
    Task<Story?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Story?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Story?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Story>> GetPageAsync(
        string? search,
        string? category,
        int skip,
        int take,
        string sort,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Story story, CancellationToken cancellationToken = default);
    void Remove(Story story);
}
