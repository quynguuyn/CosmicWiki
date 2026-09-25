using CosmicWiki.Discovery.Application.Abstractions;
using CosmicWiki.Discovery.Application.Abstractions.Persistence;
using CosmicWiki.Discovery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Discovery.Infrastructure.Persistence.Repositories;

public sealed class SearchDocumentRepository
    : ISearchDocumentRepository
{
    private readonly DiscoveryDbContext _dbContext;

    public SearchDocumentRepository(
        DiscoveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountAsync(
        string? search,
        string? sourceType,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(search, sourceType);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SearchDocument>> SearchAsync(
        string? search,
        string? sourceType,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(search, sourceType);

        return await query
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<SearchDocument> BuildQuery(
        string? search,
        string? sourceType)
    {
        IQueryable<SearchDocument> query =
            _dbContext.SearchDocuments;

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            sourceType = sourceType.Trim();

            query = query.Where(x =>
                x.SourceType == sourceType);
        }

        return query;
    }
}