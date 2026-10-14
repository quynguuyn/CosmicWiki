using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.RemovePermission;

public sealed record RemoveAdminPermissionCommand(Guid RoleId, Guid PermissionId)
    : IRequest<bool>;
