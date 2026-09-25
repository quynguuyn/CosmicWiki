using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CosmicWiki.Auth.Application.Features.Authentication.Login;
using CosmicWiki.Auth.Application.Features.Authentication.Register;
using CosmicWiki.Auth.Application.Features.Authentication.GetCurrentUser;
using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Auth.Api.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
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

        return Ok(result);
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
}