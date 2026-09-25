using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record RadialVelocityMeasurement
{
    private RadialVelocityMeasurement()
    {
    }
    
    private RadialVelocityMeasurement(
        double valueKmPerSecond,
        MeasurementUncertainty? uncertainty,
        WavelengthBand? wavelengthBand,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        ValueKmPerSecond = valueKmPerSecond;
        Uncertainty = uncertainty;
        WavelengthBand = wavelengthBand;
        Quality = quality;
        ReferenceId = referenceId;
    }

    /// Heliocentric radial velocity in km/s.
    public double ValueKmPerSecond { get; }

    public MeasurementUncertainty? Uncertainty { get; }

    public WavelengthBand? WavelengthBand { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static RadialVelocityMeasurement Create(
        double valueKmPerSecond,
        MeasurementUncertainty? uncertainty = null,
        WavelengthBand? wavelengthBand = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        return new RadialVelocityMeasurement(
            valueKmPerSecond,
            uncertainty,
            wavelengthBand,
            quality,
            referenceId);
    }
}