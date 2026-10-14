using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetRolePermissions;

public sealed record GetRolePermissionsQuery(Guid RoleId)
    : IRequest<IReadOnlyCollection<RolePermissionResponse>?>;

public sealed record RolePermissionResponse(
    Guid PermissionId,
    string Code,
    string Name,
    string? Description);
