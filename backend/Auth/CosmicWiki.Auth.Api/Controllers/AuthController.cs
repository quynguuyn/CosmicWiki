using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CosmicWiki.Auth.Application.Features.Authentication.Login;
using CosmicWiki.Auth.Application.Features.Authentication.Register;
using CosmicWiki.Auth.Application.Features.Authentication.GetCurrentUser;
using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Auth.Api.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using CosmicWiki.Auth.Application.Features.Authentication.Refresh;
using CosmicWiki.Auth.Application.Features.Authentication.Logout;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CosmicWiki.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        ISender sender,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _sender = sender;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Refresh(
        CancellationToken cancellationToken)
    {
        LoginResponse result;
        try
        {
            result = await _sender.Send(
                new RefreshCommand(
                    Request.Cookies[RefreshTokenCookieName] ?? string.Empty),
                cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            Response.Cookies.Delete(
                RefreshTokenCookieName,
                GetRefreshTokenCookieOptions());
            throw;
        }

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new LogoutCommand(Request.Cookies[RefreshTokenCookieName]),
            cancellationToken);

        Response.Cookies.Delete(
            RefreshTokenCookieName,
            GetRefreshTokenCookieOptions());

        return NoContent();
    }
    
    [HttpGet("getCurrentUser")]
    [Authorize(Policy = Permissions.AuthRead)]
    [ProducesResponseType(typeof(GetCurrentUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetCurrentUserResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCurrentUserQuery(),
            cancellationToken);

        return Ok(result);
    }

    private const string RefreshTokenCookieName = "cosmicwiki_refresh_token";

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var expirationDays = GetRefreshTokenExpirationDays();
        var options = GetRefreshTokenCookieOptions();
        options.Expires = DateTimeOffset.UtcNow.AddDays(expirationDays);
        options.MaxAge = TimeSpan.FromDays(expirationDays);

        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            options);
    }

    private CookieOptions GetRefreshTokenCookieOptions() =>
        new()
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment() || Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        };

    private int GetRefreshTokenExpirationDays()
    {
        var configuredDays = _configuration["RefreshToken:ExpirationDays"];
        if (configuredDays is null)
        {
            return 7;
        }

        if (int.TryParse(configuredDays, out var days) && days > 0)
        {
            return days;
        }

        throw new InvalidOperationException(
            "Refresh token expiration days must be a positive integer.");
    }
}