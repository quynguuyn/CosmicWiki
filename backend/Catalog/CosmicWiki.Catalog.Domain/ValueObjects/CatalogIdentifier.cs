namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record CatalogIdentifier
{
    private CatalogIdentifier(
        string catalog,
        string value)
    {
        Catalog = catalog;
        Value = value;
    }

    public string Catalog { get; }

    public string Value { get; }

    public static CatalogIdentifier Create(
        string catalog,
        string value)
    {
        if (string.IsNullOrWhiteSpace(catalog))
            throw new ArgumentException(
                "Catalog cannot be empty.",
                nameof(catalog));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Identifier value cannot be empty.",
                nameof(value));

        return new CatalogIdentifier(
            catalog.Trim(),
            value.Trim());
    }

    public override string ToString()
        => $"{Catalog} {Value}";
    
    public bool Matches(CatalogIdentifier other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return string.Equals(
                   Catalog,
                   other.Catalog,
                   StringComparison.OrdinalIgnoreCase)
               && string.Equals(
                   Value,
                   other.Value,
                   StringComparison.OrdinalIgnoreCase);
    }
}