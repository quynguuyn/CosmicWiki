using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record SpectralClassification
{
    public SpectralClassification()
    {
    }
    
    private SpectralClassification(
        string rawType,
        SpectralTemperatureClass? temperatureClass,
        string? subtype,
        string? luminosityClass,
        IReadOnlyCollection<string> peculiarities,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        RawType = rawType;
        TemperatureClass = temperatureClass;
        Subtype = subtype;
        LuminosityClass = luminosityClass;
        Peculiarities = peculiarities;
        Quality = quality;
        ReferenceId = referenceId;
    }

    /// Original spectral classification as provided by the source.
    public string RawType { get; }

    /// Parsed temperature/spectral class, if recognized.
    public SpectralTemperatureClass? TemperatureClass { get; }

    /// Spectral subtype, preserved as text because source notation
    /// is not guaranteed to be a simple integer.
    public string? Subtype { get; }

    /// Luminosity class as source notation, e.g. V, III, IIb, III/IV.
    public string? LuminosityClass { get; }

    /// Parsed or preserved spectral peculiarities/annotations.
    public IReadOnlyCollection<string> Peculiarities { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static SpectralClassification Create(
        string rawType,
        SpectralTemperatureClass? temperatureClass = null,
        string? subtype = null,
        string? luminosityClass = null,
        IReadOnlyCollection<string>? peculiarities = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (string.IsNullOrWhiteSpace(rawType))
            throw new ArgumentException(
                "Raw spectral type cannot be empty.",
                nameof(rawType));

        return new SpectralClassification(
            rawType.Trim(),
            temperatureClass,
            subtype?.Trim(),
            luminosityClass?.Trim(),
            peculiarities ?? Array.Empty<string>(),
            quality,
            referenceId);
    }
}