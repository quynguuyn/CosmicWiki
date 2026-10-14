using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.Logout;

public sealed record LogoutCommand(
    string? RefreshToken) : IRequest;
