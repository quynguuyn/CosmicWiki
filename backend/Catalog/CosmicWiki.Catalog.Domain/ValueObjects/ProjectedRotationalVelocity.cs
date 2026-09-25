namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record ProjectedRotationalVelocity
{
    public ProjectedRotationalVelocity()
    {
    }
    
    private ProjectedRotationalVelocity(
        double value,
        MeasurementUncertainty? uncertainty,
        int? measurementCount,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        Value = value;
        Uncertainty = uncertainty;
        MeasurementCount = measurementCount;
        Quality = quality;
        ReferenceId = referenceId;
    }

    /// <summary>
    /// Projected rotational velocity v sin(i), in km/s.
    /// </summary>
    public double Value { get; }

    public MeasurementUncertainty? Uncertainty { get; }

    public int? MeasurementCount { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static ProjectedRotationalVelocity Create(
        double value,
        MeasurementUncertainty? uncertainty = null,
        int? measurementCount = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        if (measurementCount is < 0)
            throw new ArgumentOutOfRangeException(
                nameof(measurementCount));

        return new ProjectedRotationalVelocity(
            value,
            uncertainty,
            measurementCount,
            quality,
            referenceId);
    }
}