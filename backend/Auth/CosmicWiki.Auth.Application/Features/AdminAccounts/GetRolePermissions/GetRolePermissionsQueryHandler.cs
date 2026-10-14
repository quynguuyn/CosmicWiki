using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetRolePermissions;

public sealed class GetRolePermissionsQueryHandler(IAdminAccountRepository repository)
    : IRequestHandler<GetRolePermissionsQuery, IReadOnlyCollection<RolePermissionResponse>?>
{
    public async Task<IReadOnlyCollection<RolePermissionResponse>?> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await repository.GetRolePermissionsAsync(
            request.RoleId,
            cancellationToken);
        return permissions?.Select(x => new RolePermissionResponse(
            x.PermissionId, x.Code, x.Name, x.Description)).ToArray();
    }
}
