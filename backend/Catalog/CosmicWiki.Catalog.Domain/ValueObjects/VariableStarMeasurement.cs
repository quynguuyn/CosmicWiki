namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record VariableStarMeasurement
{
    private VariableStarMeasurement(
        string variabilityType,
        double? maximumBrightness,
        double? minimumBrightness,
        string? photometricBand,
        double? periodDays,
        double? epochJulianDay,
        double? characteristicPercent,
        Guid? referenceId)
    {
        VariabilityType = variabilityType;
        MaximumBrightness = maximumBrightness;
        MinimumBrightness = minimumBrightness;
        PhotometricBand = photometricBand;
        PeriodDays = periodDays;
        EpochJulianDay = epochJulianDay;
        CharacteristicPercent = characteristicPercent;
        ReferenceId = referenceId;
    }

    public string VariabilityType { get; }

    public double? MaximumBrightness { get; }

    public double? MinimumBrightness { get; }

    public string? PhotometricBand { get; }

    /// Period in days.
    public double? PeriodDays { get; }

    /// Epoch in Julian Days.
    public double? EpochJulianDay { get; }

    /// Eclipse duration for Algol-type variables;
    /// rising time for other variable types.
    public double? CharacteristicPercent { get; }

    public Guid? ReferenceId { get; }

    public static VariableStarMeasurement Create(
        string variabilityType,
        double? maximumBrightness = null,
        double? minimumBrightness = null,
        string? photometricBand = null,
        double? periodDays = null,
        double? epochJulianDay = null,
        double? characteristicPercent = null,
        Guid? referenceId = null)
    {
        if (string.IsNullOrWhiteSpace(variabilityType))
            throw new ArgumentException(
                "Variability type cannot be empty.",
                nameof(variabilityType));

        if (periodDays is <= 0)
            throw new ArgumentOutOfRangeException(nameof(periodDays));

        if (characteristicPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(
                nameof(characteristicPercent));

        return new VariableStarMeasurement(
            variabilityType.Trim(),
            maximumBrightness,
            minimumBrightness,
            photometricBand?.Trim(),
            periodDays,
            epochJulianDay,
            characteristicPercent,
            referenceId);
    }
}