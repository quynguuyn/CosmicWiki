namespace CosmicWiki.Auth.Domain.ValueObjects;

public sealed record Username
{
    private Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Username cannot be empty.",
                nameof(value));

        value = value.Trim();

        if (value.Length < 3)
            throw new ArgumentException(
                "Username must contain at least 3 characters.",
                nameof(value));

        if (value.Length > 50)
            throw new ArgumentException(
                "Username cannot exceed 50 characters.",
                nameof(value));

        return new Username(value);
    }

    public override string ToString()
        => Value;
}