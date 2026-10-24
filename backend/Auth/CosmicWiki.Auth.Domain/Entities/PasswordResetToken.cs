namespace CosmicWiki.Auth.Domain.Entities;

public sealed class PasswordResetToken
{
    private PasswordResetToken() { }

    private PasswordResetToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public static PasswordResetToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Expiration must be UTC.", nameof(expiresAtUtc));

        return new PasswordResetToken(Guid.NewGuid(), userId, tokenHash, expiresAtUtc);
    }

    public bool IsUsable(DateTime nowUtc) => UsedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void MarkUsed()
    {
        if (UsedAtUtc is not null)
            throw new InvalidOperationException("Password reset token was already used.");
        UsedAtUtc = DateTime.UtcNow;
    }
}
