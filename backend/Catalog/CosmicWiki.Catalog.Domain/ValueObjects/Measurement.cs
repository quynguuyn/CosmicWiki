using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record Measurement
{
    public Measurement()
    {
    }
    
    private Measurement(
        double value,
        string unit,
        MeasurementUncertainty? uncertainty,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        Value = value;
        Unit = unit;
        Uncertainty = uncertainty;
        Quality = quality;
        ReferenceId = referenceId;
    }

    public double Value { get; }

    public string Unit { get; }

    public MeasurementUncertainty? Uncertainty { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static Measurement Create(
        double value,
        string unit,
        MeasurementUncertainty? uncertainty = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException(
                "Unit cannot be empty.",
                nameof(unit));

        return new Measurement(
            value,
            unit.Trim(),
            uncertainty,
            quality,
            referenceId);
    }
}