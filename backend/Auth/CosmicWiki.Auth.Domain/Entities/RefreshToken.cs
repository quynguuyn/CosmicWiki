namespace CosmicWiki.Auth.Domain.Entities;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        Guid familyId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshToken Create(
        Guid userId,
        Guid familyId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (familyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Token family ID cannot be empty.",
                nameof(familyId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException(
                "Token hash cannot be empty.",
                nameof(tokenHash));
        }

        if (expiresAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Token expiration must be UTC.",
                nameof(expiresAtUtc));
        }

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            familyId,
            tokenHash,
            expiresAtUtc);
    }

    public bool IsActive(DateTime nowUtc) =>
        RevokedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void Revoke(string? replacedByTokenHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
