namespace CosmicWiki.Catalog.Domain.ValueObjects;

public sealed record AstronomicalObjectType
{
    private AstronomicalObjectType(
        string code,
        string name,
        string? description)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Source/catalogue object-type code.
    /// Example: "Star", "V*", "Ce*", "WD*".
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Human-readable object type name.
    /// </summary>
    public string Name { get; }

    public string? Description { get; }

    public static AstronomicalObjectType Create(
        string code,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Object type code cannot be empty.",
                nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Object type name cannot be empty.",
                nameof(name));

        return new AstronomicalObjectType(
            code.Trim(),
            name.Trim(),
            description?.Trim());
    }

    public override string ToString() => $"{Code} - {Name}";
}