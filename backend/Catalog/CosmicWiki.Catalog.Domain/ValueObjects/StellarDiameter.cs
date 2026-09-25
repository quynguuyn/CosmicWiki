namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record StellarDiameter
{
    public StellarDiameter()
    {
    }
    
    private StellarDiameter(
        double value,
        string unit,
        MeasurementUncertainty? uncertainty,
        string? filterOrWavelength,
        string? method,
        MeasurementQuality quality,
        Guid? referenceId)
    {
        Value = value;
        Unit = unit;
        Uncertainty = uncertainty;
        FilterOrWavelength = filterOrWavelength;
        Method = method;
        Quality = quality;
        ReferenceId = referenceId;
    }

    public double Value { get; }

    public string Unit { get; }

    public MeasurementUncertainty? Uncertainty { get; }

    public string? FilterOrWavelength { get; }

    public string? Method { get; }

    public MeasurementQuality Quality { get; }

    public Guid? ReferenceId { get; }

    public static StellarDiameter Create(
        double value,
        string unit,
        MeasurementUncertainty? uncertainty = null,
        string? filterOrWavelength = null,
        string? method = null,
        MeasurementQuality quality = MeasurementQuality.Unknown,
        Guid? referenceId = null)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException(
                "Unit cannot be empty.",
                nameof(unit));

        return new StellarDiameter(
            value,
            unit.Trim(),
            uncertainty,
            filterOrWavelength?.Trim(),
            method?.Trim(),
            quality,
            referenceId);
    }
}