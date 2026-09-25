namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record LogSurfaceGravity
{
    private LogSurfaceGravity(double value)
    {
        Value = value;
    }

    /// <summary>
    /// log10(g), where g is expressed in cm/s².
    /// </summary>
    public double Value { get; }

    public static LogSurfaceGravity Create(double value)
    {
        return new LogSurfaceGravity(value);
    }
}