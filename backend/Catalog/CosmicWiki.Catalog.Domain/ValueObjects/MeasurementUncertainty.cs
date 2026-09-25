namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record MeasurementUncertainty
{
    public MeasurementUncertainty()
    {
        
    }
    
    private MeasurementUncertainty(
        double? negative,
        double? positive)
    {
        Negative = negative;
        Positive = positive;
    }

    public double? Negative { get; }

    public double? Positive { get; }

    public static MeasurementUncertainty Symmetric(
        double error)
    {
        if (error < 0)
            throw new ArgumentOutOfRangeException(nameof(error));

        return new MeasurementUncertainty(
            error,
            error);
    }

    public static MeasurementUncertainty Asymmetric(
        double negative,
        double positive)
    {
        if (negative < 0)
            throw new ArgumentOutOfRangeException(nameof(negative));

        if (positive < 0)
            throw new ArgumentOutOfRangeException(nameof(positive));

        return new MeasurementUncertainty(
            negative,
            positive);
    }
}