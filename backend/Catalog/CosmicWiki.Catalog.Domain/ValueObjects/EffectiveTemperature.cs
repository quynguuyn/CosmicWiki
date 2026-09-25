namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record EffectiveTemperature
{
    private EffectiveTemperature(double kelvin)
    {
        Kelvin = kelvin;
    }

    public double Kelvin { get; }

    public static EffectiveTemperature Create(double kelvin)
    {
        if (kelvin <= 0)
            throw new ArgumentOutOfRangeException(nameof(kelvin));

        return new EffectiveTemperature(kelvin);
    }
}