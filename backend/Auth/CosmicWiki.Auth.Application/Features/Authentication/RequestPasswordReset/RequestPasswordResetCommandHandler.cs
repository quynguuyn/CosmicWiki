using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler(
    IPasswordRecoveryService passwordRecovery)
    : IRequestHandler<RequestPasswordResetCommand, RequestPasswordResetResponse>
{
    public async Task<RequestPasswordResetResponse> Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.", nameof(request.Email));

        var result = await passwordRecovery.RequestResetAsync(
            request.Email,
            cancellationToken);

        return new RequestPasswordResetResponse(
            "If an account exists for that email, password reset instructions have been sent.",
            result.EmailDeliveryEnabled,
            result.DevelopmentResetUrl);
    }
}
