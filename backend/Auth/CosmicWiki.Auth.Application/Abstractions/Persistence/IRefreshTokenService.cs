namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IRefreshTokenService
{
    Task<RefreshTokenCreation> CreateAsync(
        Guid userId,
        bool rememberMe,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenRotation?> RotateAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string token,
        CancellationToken cancellationToken = default);
}
