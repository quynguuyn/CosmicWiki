namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record ErrorEllipse
{
    private ErrorEllipse()
    {
    }
    
    private ErrorEllipse(
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

    public static ErrorEllipse Create(
        double majorAxis,
        double minorAxis,
        double positionAngle)
    {
        if (majorAxis < 0)
            throw new ArgumentOutOfRangeException(nameof(majorAxis));

        if (minorAxis < 0)
            throw new ArgumentOutOfRangeException(nameof(minorAxis));

        if (majorAxis < minorAxis)
            throw new ArgumentException(
                "Major axis cannot be smaller than minor axis.",
                nameof(majorAxis));

        if (positionAngle < 0 || positionAngle >= 180)
            throw new ArgumentOutOfRangeException(
                nameof(positionAngle));

        return new ErrorEllipse(
            majorAxis,
            minorAxis,
            positionAngle);
    }
}