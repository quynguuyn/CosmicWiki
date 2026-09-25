namespace CosmicWiki.Auth.Application.Features.Authentication.Register;

public sealed record RegisterResponse(
    Guid UserId,
    string Username,
    string Email);