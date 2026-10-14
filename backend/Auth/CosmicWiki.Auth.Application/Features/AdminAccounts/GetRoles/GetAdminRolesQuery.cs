using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetRoles;

public sealed record GetAdminRolesQuery : IRequest<IReadOnlyCollection<AdminRoleResponse>>;

public sealed record AdminRoleResponse(Guid RoleId, string Name, string? Description);
