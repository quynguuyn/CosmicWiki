using CosmicWiki.Discovery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Discovery.Infrastructure.Persistence;

internal static class SearchDocumentUpsert
{
    public static async Task ExecuteAsync(
        DiscoveryDbContext dbContext,
        Guid sourceId,
        string sourceType,
        string name,
        string? description,
        string? imageUrl,
        DateTime occurredAtUtc,
        string? catalog,
        string? catalogValue,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.SearchDocuments.SingleOrDefaultAsync(
            x => x.SourceType == sourceType && x.SourceId == sourceId,
            cancellationToken);

        if (document is null)
        {
            dbContext.SearchDocuments.Add(SearchDocument.Create(
                sourceId,
                sourceType,
                name,
                description,
                imageUrl,
                occurredAtUtc,
                catalog,
                catalogValue));
        }
        else
        {
            document.Update(
                name,
                description,
                imageUrl,
                occurredAtUtc,
                catalog,
                catalogValue);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
