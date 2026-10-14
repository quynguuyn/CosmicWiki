using CosmicWiki.Auth.Application.Features.AdminAccounts.AssignPermission;
using CosmicWiki.Auth.Application.Features.AdminAccounts.GetAccounts;
using CosmicWiki.Auth.Application.Features.AdminAccounts.GetPermissions;
using CosmicWiki.Auth.Application.Features.AdminAccounts.GetRolePermissions;
using CosmicWiki.Auth.Application.Features.AdminAccounts.GetRoles;
using CosmicWiki.Auth.Application.Features.AdminAccounts.RemovePermission;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Auth.Api.Controllers;

[ApiController]
[Route("api/auth/admin")]
[Authorize(Policy = Permissions.AuthWrite)]
public sealed class AdminAccountsController(
    ISender sender) : ControllerBase
{
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string? role,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAdminAccountsQuery(role), cancellationToken));

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAdminRolesQuery(), cancellationToken));

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAdminPermissionsQuery(), cancellationToken));

    [HttpGet("roles/{roleId:guid}/permissions")]
    public async Task<IActionResult> GetRolePermissions(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(
            new GetRolePermissionsQuery(roleId), cancellationToken);
        return permissions is null ? NotFound() : Ok(permissions);
    }

    [HttpPut("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> AssignPermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken) =>
        await sender.Send(
            new AssignAdminPermissionCommand(roleId, permissionId), cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpDelete("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemovePermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken) =>
        await sender.Send(
            new RemoveAdminPermissionCommand(roleId, permissionId), cancellationToken)
            ? NoContent()
            : NotFound();
}
