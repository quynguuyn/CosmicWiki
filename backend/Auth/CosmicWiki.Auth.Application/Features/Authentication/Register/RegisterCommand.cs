using MediatR;

namespace CosmicWiki.Auth.Application.Features.Authentication.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password) : IRequest<RegisterResponse>;