namespace CosmicWiki.Catalog.Application.Research;

public interface IAstronomicalResearchProvider
{
    Task<IReadOnlyList<AstronomicalResearchCandidate>> FindCandidatesAsync(
        CancellationToken cancellationToken = default);
}
