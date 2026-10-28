using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IPasswordRecoveryService passwordRecovery)
    : IRequestHandler<ResetPasswordCommand, bool>
{
    public Task<bool> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("A reset token is required.", nameof(request.Token));
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ArgumentException("A new password is required.", nameof(request.NewPassword));

        return passwordRecovery.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            cancellationToken);
    }
}
