using MediatR;
using CosmicWiki.Auth.Application.Features.Authentication.Login;

namespace CosmicWiki.Auth.Application.Features.Authentication.Refresh;

public sealed record RefreshCommand(
    string RefreshToken) : IRequest<LoginResponse>;
