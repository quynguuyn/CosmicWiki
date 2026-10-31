namespace CosmicWiki.Catalog.Application.Research;

public interface IAstronomicalResearchProvider
{
    Task<AstronomicalResearchSourceStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);

    Task<AstronomicalResearchPage> FindCandidatesAsync(
        string? afterMainId,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed record AstronomicalResearchSourceStatus(
    bool IsReachable,
    string? Message,
    DateTime CheckedAtUtc);
