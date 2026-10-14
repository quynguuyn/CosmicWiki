using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetPermissions;

public sealed record GetAdminPermissionsQuery : IRequest<IReadOnlyCollection<AdminPermissionResponse>>;

public sealed record AdminPermissionResponse(
    Guid PermissionId,
    string Code,
    string Name,
    string? Description);
