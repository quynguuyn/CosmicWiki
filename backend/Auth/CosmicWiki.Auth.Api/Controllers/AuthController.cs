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
using CosmicWiki.Auth.Application.Features.Authentication.RequestPasswordReset;
using CosmicWiki.Auth.Application.Features.Authentication.ResetPassword;
using Microsoft.AspNetCore.Hosting;

namespace CosmicWiki.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        ISender sender,
        IWebHostEnvironment environment)
    {
        _sender = sender;
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
        SetRefreshTokenCookie(result);

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

        SetRefreshTokenCookie(result);
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

    [HttpPost("password/forgot")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Accepted(new
        {
            result.Message,
            result.EmailDeliveryEnabled,
            developmentResetUrl = _environment.IsDevelopment()
                ? result.DevelopmentResetUrl
                : null
        });
    }

    [HttpPost("password/reset")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitingExtensions.LoginPolicy)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var reset = await _sender.Send(command, cancellationToken);
        return reset ? NoContent() : BadRequest("The reset link is invalid or expired.");
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

    private void SetRefreshTokenCookie(LoginResponse result)
    {
        var options = GetRefreshTokenCookieOptions();
        if (result.RememberMe)
        {
            options.Expires = new DateTimeOffset(DateTime.SpecifyKind(
                result.RefreshTokenExpiresAtUtc,
                DateTimeKind.Utc));
            options.MaxAge = options.Expires - DateTimeOffset.UtcNow;
        }

        Response.Cookies.Append(
            RefreshTokenCookieName,
            result.RefreshToken,
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
}