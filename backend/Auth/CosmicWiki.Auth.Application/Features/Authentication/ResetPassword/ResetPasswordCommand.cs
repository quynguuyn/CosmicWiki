using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword)
    : IRequest<bool>;
