using CosmicWiki.Catalog.Application.Research;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public interface IAstronomicalResearchCandidateProcessor
{
    Task<AstronomicalResearchPageResult> ProcessPageAsync(
        IReadOnlyCollection<AstronomicalResearchCandidate> candidates,
        CancellationToken cancellationToken);
}

public sealed record AstronomicalResearchPageResult(
    int CreatedCount,
    int ExistingCount);
