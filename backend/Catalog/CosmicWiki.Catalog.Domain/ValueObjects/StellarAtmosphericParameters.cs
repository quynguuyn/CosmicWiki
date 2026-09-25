using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record StellarAtmosphericParameters
{
    public StellarAtmosphericParameters()
    {
    }
    
    private StellarAtmosphericParameters(
        EffectiveTemperature? effectiveTemperature,
        LogSurfaceGravity? surfaceGravity,
        Metallicity? metallicity,
        MetallicityFlags metallicityFlags,
        string? comparisonStar,
        string? catalogNumber,
        Guid? referenceId)
    {
        EffectiveTemperature = effectiveTemperature;
        SurfaceGravity = surfaceGravity;
        Metallicity = metallicity;
        MetallicityFlags = metallicityFlags;
        ComparisonStar = comparisonStar;
        CatalogNumber = catalogNumber;
        ReferenceId = referenceId;
    }

    public EffectiveTemperature? EffectiveTemperature { get; }

    public LogSurfaceGravity? SurfaceGravity { get; }

    public Metallicity? Metallicity { get; }

    public MetallicityFlags MetallicityFlags { get; }

    public string? ComparisonStar { get; }

    public string? CatalogNumber { get; }

    public Guid? ReferenceId { get; }

    public static StellarAtmosphericParameters Create(
        EffectiveTemperature? effectiveTemperature = null,
        LogSurfaceGravity? surfaceGravity = null,
        Metallicity? metallicity = null,
        MetallicityFlags metallicityFlags = MetallicityFlags.None,
        string? comparisonStar = null,
        string? catalogNumber = null,
        Guid? referenceId = null)
    {
        if (effectiveTemperature is null &&
            surfaceGravity is null &&
            metallicity is null)
        {
            throw new ArgumentException(
                "At least one stellar atmospheric parameter is required.");
        }

        return new StellarAtmosphericParameters(
            effectiveTemperature,
            surfaceGravity,
            metallicity,
            metallicityFlags,
            comparisonStar?.Trim(),
            catalogNumber?.Trim(),
            referenceId);
    }
}