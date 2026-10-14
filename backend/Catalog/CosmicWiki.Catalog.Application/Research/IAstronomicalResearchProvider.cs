namespace CosmicWiki.Catalog.Application.Research;

public interface IAstronomicalResearchProvider
{
    Task<AstronomicalResearchPage> FindCandidatesAsync(
        string? afterMainId,
        int pageSize,
        CancellationToken cancellationToken = default);
}
