using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record DistanceMeasurement
{
    private DistanceMeasurement()
    {
    }
    
    private DistanceMeasurement(
        double value,
        string unit,
        MeasurementUncertainty? uncertainty,
        DistanceMethod method,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        Value = value;
        Unit = unit;
        Uncertainty = uncertainty;
        Method = method;
        Quality = quality;
        ReferenceId = referenceId;
    }

    public double Value { get; }

    public string Unit { get; }

    public MeasurementUncertainty? Uncertainty { get; }

    public DistanceMethod Method { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static DistanceMeasurement Create(
        double value,
        string unit,
        DistanceMethod method,
        MeasurementUncertainty? uncertainty = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException(
                "Unit cannot be empty.",
                nameof(unit));

        return new DistanceMeasurement(
            value,
            unit.Trim(),
            uncertainty,
            method,
            quality,
            referenceId);
    }
}