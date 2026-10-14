namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IRefreshTokenService
{
    Task<string> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenRotation?> RotateAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string token,
        CancellationToken cancellationToken = default);
}
