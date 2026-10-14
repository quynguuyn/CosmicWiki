using CosmicWiki.SharedKernel.Abstractions;
using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Domain.Entities;

public sealed class AstronomicalObject : Entity<Guid>
{
    private AstronomicalObject(
        Guid id,
        string? name,
        AstronomicalObjectKind kind,
        string? objectType,
        string? catalog,
        string? catalogValue,
        double? rightAscensionDegrees,
        double? declinationDegrees)
        : base(id)
    {
        Name = name;
        Kind = kind;
        ObjectType = objectType;
        Catalog = catalog;
        CatalogValue = catalogValue;
        RightAscensionDegrees = rightAscensionDegrees;
        DeclinationDegrees = declinationDegrees;
    }

    public string? Name { get; private set; }

    public AstronomicalObjectKind Kind { get; private set; }

    public string? ObjectType { get; private set; }

    public string? Catalog { get; private set; }

    public string? CatalogValue { get; private set; }

    public double? RightAscensionDegrees { get; private set; }

    public double? DeclinationDegrees { get; private set; }

    public static AstronomicalObject Create()
    {
        return Create(Guid.NewGuid());
    }

    public static AstronomicalObject Create(Guid id)
    {
        return Create(
            id,
            null,
            AstronomicalObjectKind.Other,
            null,
            null,
            null,
            null,
            null);
    }

    public static AstronomicalObject Create(
        string? name,
        AstronomicalObjectKind kind,
        string? objectType,
        string? catalog,
        string? catalogValue,
        double? rightAscensionDegrees = null,
        double? declinationDegrees = null)
    {
        return Create(
            Guid.NewGuid(),
            name,
            kind,
            objectType,
            catalog,
            catalogValue,
            rightAscensionDegrees,
            declinationDegrees);
    }

    public static AstronomicalObject Create(
        Guid id,
        string? name,
        AstronomicalObjectKind kind,
        string? objectType,
        string? catalog,
        string? catalogValue,
        double? rightAscensionDegrees = null,
        double? declinationDegrees = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(catalog) !=
            string.IsNullOrWhiteSpace(catalogValue))
            throw new ArgumentException(
                "Catalog and catalog value must be provided together.");
        if ((rightAscensionDegrees is null) != (declinationDegrees is null))
            throw new ArgumentException("Both coordinate values must be provided together.");
        if (rightAscensionDegrees is < 0 or >= 360)
            throw new ArgumentOutOfRangeException(nameof(rightAscensionDegrees));
        if (declinationDegrees is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(declinationDegrees));

        return new AstronomicalObject(
            id,
            name.Trim(),
            kind,
            objectType?.Trim(),
            catalog?.Trim(),
            catalogValue?.Trim(),
            rightAscensionDegrees,
            declinationDegrees);
    }

    public void UpdatePrimaryCatalogIdentifier(
        string? catalog,
        string? catalogValue)
    {
        Catalog = string.IsNullOrWhiteSpace(catalog) ? null : catalog.Trim();
        CatalogValue = string.IsNullOrWhiteSpace(catalogValue)
            ? null
            : catalogValue.Trim();
    }

    public void Rename(string? name)
    {
        Name = name?.Trim();
    }

    public void UpdateClassification(
        AstronomicalObjectKind kind,
        string? objectType)
    {
        Kind = kind;
        ObjectType = objectType?.Trim();
    }
}