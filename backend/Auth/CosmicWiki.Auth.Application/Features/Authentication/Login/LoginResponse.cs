namespace CosmicWiki.Auth.Application.Features.Authentication.Login;

public sealed record LoginResponse(
    string AccessToken,
    int ExpiresIn);