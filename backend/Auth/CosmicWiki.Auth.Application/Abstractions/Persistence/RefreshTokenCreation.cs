namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public sealed record RefreshTokenCreation(
    string RefreshToken,
    DateTime ExpiresAtUtc,
    bool RememberMe);
