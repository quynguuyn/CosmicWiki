namespace CosmicWiki.Catalog.Domain.Enums;

public sealed record PhotometryFlags
{
    private PhotometryFlags(
        bool isJoined,
        string? variabilityCode)
    {
        IsJoined = isJoined;
        VariabilityCode = variabilityCode;
    }

    public bool IsJoined { get; }

    public string? VariabilityCode { get; }

    public static PhotometryFlags Create(
        bool isJoined = false,
        string? variabilityCode = null)
    {
        return new PhotometryFlags(
            isJoined,
            variabilityCode);
    }
}