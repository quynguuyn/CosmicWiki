using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record ProperMotion
{
    private ProperMotion()
    {
    }
    
    private ProperMotion(
        double rightAscension,
        double declination,
        ErrorEllipse? uncertainty,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        RightAscension = rightAscension;
        Declination = declination;
        Uncertainty = uncertainty;
        Quality = quality;
        ReferenceId = referenceId;
    }

    /// <summary>
    /// μRA × cos(Dec), expressed in mas/year.
    /// </summary>
    public double RightAscension { get; }

    /// <summary>
    /// μDec, expressed in mas/year.
    /// </summary>
    public double Declination { get; }

    public ErrorEllipse? Uncertainty { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static ProperMotion Create(
        double rightAscension,
        double declination,
        ErrorEllipse? uncertainty = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        return new ProperMotion(
            rightAscension,
            declination,
            uncertainty,
            quality,
            referenceId);
    }
}