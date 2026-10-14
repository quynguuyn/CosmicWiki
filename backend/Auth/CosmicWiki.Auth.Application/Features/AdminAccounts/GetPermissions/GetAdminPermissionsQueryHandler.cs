using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetPermissions;

public sealed class GetAdminPermissionsQueryHandler(IAdminAccountRepository repository)
    : IRequestHandler<GetAdminPermissionsQuery, IReadOnlyCollection<AdminPermissionResponse>>
{
    public async Task<IReadOnlyCollection<AdminPermissionResponse>> Handle(
        GetAdminPermissionsQuery request,
        CancellationToken cancellationToken) =>
        (await repository.GetPermissionsAsync(cancellationToken))
        .Select(x => new AdminPermissionResponse(
            x.PermissionId, x.Code, x.Name, x.Description))
        .ToArray();
}
