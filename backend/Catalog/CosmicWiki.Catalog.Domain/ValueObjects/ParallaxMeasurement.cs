using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record ParallaxMeasurement
{
    private ParallaxMeasurement()
    {
    }

    private ParallaxMeasurement(
        double valueMas,
        MeasurementUncertainty? uncertainty,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        ValueMas = valueMas;
        Uncertainty = uncertainty;
        Quality = quality;
        ReferenceId = referenceId;
    }

    /// Trigonometric parallax in milliarcseconds.
    public double ValueMas { get; private set; }

    public MeasurementUncertainty? Uncertainty { get; private set; }

    public MeasurementQuality Quality { get; private set; }

    public Guid? ReferenceId { get; private set; }

    public static ParallaxMeasurement Create(
        double valueMas,
        MeasurementUncertainty? uncertainty = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (valueMas < 0)
            throw new ArgumentOutOfRangeException(nameof(valueMas));

        return new ParallaxMeasurement(
            valueMas,
            uncertainty,
            quality,
            referenceId);
    }
}