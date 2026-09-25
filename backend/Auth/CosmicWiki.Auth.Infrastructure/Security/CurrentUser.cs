using CosmicWiki.Auth.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Http;

namespace CosmicWiki.Auth.Infrastructure.Security;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst("sub")
                ?.Value;

            return Guid.TryParse(value, out var userId)
                ? userId
                : null;
        }
    }

    public string? Username =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirst("unique_name")
            ?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        ?? false;
}