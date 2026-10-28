namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IPasswordRecoveryService
{
    Task<PasswordResetRequestResult> RequestResetAsync(string email, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken);
}

public sealed record PasswordResetRequestResult(
    bool EmailDeliveryEnabled,
    string? DevelopmentResetUrl);
