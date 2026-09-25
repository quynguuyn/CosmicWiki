using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record PhotometricMeasurement
{
    public PhotometricMeasurement()
    {
    }
    
    private PhotometricMeasurement(
        PhotometricFilter filter,
        PhotometricSystem system,
        PhotometricValueType valueType,
        Measurement measurement,
        PhotometryFlags? flags)
    {
        Filter = filter;
        System = system;
        ValueType = valueType;
        Measurement = measurement;
        Flags = flags;
    }

    public PhotometricFilter Filter { get; }

    public PhotometricSystem System { get; }

    public PhotometricValueType ValueType { get; }

    public Measurement Measurement { get; }

    public PhotometryFlags? Flags { get; }

    public static PhotometricMeasurement Create(
        PhotometricFilter filter,
        PhotometricSystem system,
        PhotometricValueType valueType,
        Measurement measurement,
        PhotometryFlags? flags = null)
    {
        ArgumentNullException.ThrowIfNull(measurement);

        return new PhotometricMeasurement(
            filter,
            system,
            valueType,
            measurement,
            flags);
    }
}