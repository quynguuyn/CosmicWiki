using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email)
    : IRequest<RequestPasswordResetResponse>;

public sealed record RequestPasswordResetResponse(
    string Message,
    bool EmailDeliveryEnabled,
    string? DevelopmentResetUrl);
