namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public sealed record RefreshTokenRotation(
    Guid UserId,
    string RefreshToken);
