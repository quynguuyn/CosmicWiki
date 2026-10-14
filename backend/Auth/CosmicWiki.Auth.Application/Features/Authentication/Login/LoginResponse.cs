using System.Text.Json.Serialization;

namespace CosmicWiki.Auth.Application.Features.Authentication.Login;

public sealed record LoginResponse(
    string AccessToken,
    int ExpiresIn,
    [property: JsonIgnore] string RefreshToken);