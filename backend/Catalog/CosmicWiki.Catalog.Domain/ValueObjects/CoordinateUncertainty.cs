namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record CoordinateUncertainty
{
    private CoordinateUncertainty(
        double majorAxis,
        double minorAxis,
        double positionAngle)
    {
        MajorAxis = majorAxis;
        MinorAxis = minorAxis;
        PositionAngle = positionAngle;
    }

    public double MajorAxis { get; }

    public double MinorAxis { get; }

    public double PositionAngle { get; }

    public static CoordinateUncertainty Create(
        double majorAxis,
        double minorAxis,
        double positionAngle)
    {
        if (majorAxis < 0)
            throw new ArgumentOutOfRangeException(nameof(majorAxis));

        if (minorAxis < 0)
            throw new ArgumentOutOfRangeException(nameof(minorAxis));

        if (positionAngle < 0 || positionAngle >= 360)
            throw new ArgumentOutOfRangeException(
                nameof(positionAngle));

        return new CoordinateUncertainty(
            majorAxis,
            minorAxis,
            positionAngle);
    }
}