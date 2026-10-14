using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.AssignPermission;

public sealed record AssignAdminPermissionCommand(Guid RoleId, Guid PermissionId)
    : IRequest<bool>;
