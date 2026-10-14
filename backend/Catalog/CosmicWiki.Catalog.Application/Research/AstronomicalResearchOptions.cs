namespace CosmicWiki.Catalog.Application.Research;

public sealed class AstronomicalResearchOptions
{
    public bool Enabled { get; set; } = true;

    public string Endpoint { get; set; } =
        "https://simbad.u-strasbg.fr/simbad/sim-tap/sync";

    public string Query { get; set; } =
        "SELECT TOP 100 main_id, otype_txt FROM basic " +
        "WHERE otype IN ('*', 'X', 'G') " +
        "ORDER BY main_id DESC";

    public int PageSize { get; set; } = 100;
}

public sealed record AstronomicalResearchCandidate(
    string Name,
    string ObjectType,
    string Catalog,
    string CatalogValue);

public sealed record AstronomicalResearchRunResult(
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc,
    int CandidateCount,
    int ExistingCount,
    int CreatedCount);
