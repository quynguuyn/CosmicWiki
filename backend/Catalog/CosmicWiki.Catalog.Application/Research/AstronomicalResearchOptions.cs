using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Application.Research;

public sealed class AstronomicalResearchOptions
{
    public bool Enabled { get; set; } = true;

    public string Endpoint { get; set; } =
        "https://simbad.u-strasbg.fr/simbad/sim-tap/sync";

    public int PageSize { get; set; } = 500;

    public int MaxPagesPerRun { get; set; } = 20;
}

public sealed record AstronomicalResearchCandidate(
    string Name,
    AstronomicalObjectKind Kind,
    string ObjectType,
    string Catalog,
    string CatalogValue,
    double? RightAscensionDegrees,
    double? DeclinationDegrees,
    double? ParallaxMas);

public sealed record AstronomicalResearchPage(
    IReadOnlyList<AstronomicalResearchCandidate> Candidates,
    string? NextCursor,
    bool HasMore);

public sealed record AstronomicalResearchRunResult(
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc,
    int CandidateCount,
    int ExistingCount,
    int CreatedCount,
    int PagesProcessed = 0,
    bool SweepCompleted = false,
    string? NextCursor = null);
