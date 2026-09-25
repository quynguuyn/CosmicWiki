using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record AstronomicalCoordinate
{
    private AstronomicalCoordinate()
    {
    }
    
    private AstronomicalCoordinate(
        double rightAscension,
        double declination,
        CoordinateSystem coordinateSystem,
        double epoch,
        WavelengthBand? wavelengthBand,
        CoordinateUncertainty? uncertainty,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        RightAscension = rightAscension;
        Declination = declination;
        CoordinateSystem = coordinateSystem;
        Epoch = epoch;
        WavelengthBand = wavelengthBand;
        Uncertainty = uncertainty;
        Quality = quality;
        ReferenceId = referenceId;
    }

    public double RightAscension { get; }

    public double Declination { get; }

    public CoordinateSystem CoordinateSystem { get; }

    public double Epoch { get; }

    public WavelengthBand? WavelengthBand { get; }

    public CoordinateUncertainty? Uncertainty { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static AstronomicalCoordinate Create(
        double rightAscension,
        double declination,
        CoordinateSystem coordinateSystem,
        double epoch,
        WavelengthBand? wavelengthBand = null,
        CoordinateUncertainty? uncertainty = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (rightAscension < 0 || rightAscension >= 360)
            throw new ArgumentOutOfRangeException(
                nameof(rightAscension));

        if (declination < -90 || declination > 90)
            throw new ArgumentOutOfRangeException(
                nameof(declination));

        return new AstronomicalCoordinate(
            rightAscension,
            declination,
            coordinateSystem,
            epoch,
            wavelengthBand,
            uncertainty,
            quality,
            referenceId);
    }
}