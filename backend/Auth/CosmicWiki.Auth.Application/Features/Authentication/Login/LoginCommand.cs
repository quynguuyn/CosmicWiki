using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.Login;

public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<LoginResponse>;