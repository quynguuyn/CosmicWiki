using CosmicWiki.Discovery.Domain.Entities;

namespace CosmicWiki.Discovery.Application.Abstractions.Persistence;

public interface ISearchDocumentRepository
{
    Task<int> CountAsync(
        string? search,
        string? sourceType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SearchDocument>> SearchAsync(
        string? search,
        string? sourceType,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}