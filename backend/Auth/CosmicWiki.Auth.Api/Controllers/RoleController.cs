using CosmicWiki.Auth.Application.Features.Users.AssignRole;
using CosmicWiki.Auth.Application.Features.Users.RemoveRole;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Auth.Api.Controllers;

[ApiController]
[Route("api/auth/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.AuthWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new AssignRoleCommand(
                userId,
                roleId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.AuthWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new RemoveRoleCommand(
                userId,
                roleId),
            cancellationToken);

        return NoContent();
    }
}