namespace CosmicWiki.Auth.Application.Features.Authentication.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid UserId,
    string Username,
    string Email);