namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record BrightnessRange
{
    private BrightnessRange(
        double? maximum,
        double? minimum,
        string? band)
    {
        Maximum = maximum;
        Minimum = minimum;
        Band = band;
    }

    public double? Maximum { get; }
    public double? Minimum { get; }
    public string? Band { get; }

    public static BrightnessRange Create(
        double? maximum = null,
        double? minimum = null,
        string? band = null)
    {
        return new BrightnessRange(
            maximum,
            minimum,
            band?.Trim());
    }
}