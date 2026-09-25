namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record Metallicity
{
    private Metallicity(double feH)
    {
        FeH = feH;
    }

    /// <summary>
    /// [Fe/H] relative to the Sun.
    /// </summary>
    public double FeH { get; }

    public static Metallicity Create(double feH)
    {
        return new Metallicity(feH);
    }
}