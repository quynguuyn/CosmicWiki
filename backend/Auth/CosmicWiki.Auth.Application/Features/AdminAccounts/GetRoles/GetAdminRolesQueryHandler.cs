using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.GetRoles;

public sealed class GetAdminRolesQueryHandler(IAdminAccountRepository repository)
    : IRequestHandler<GetAdminRolesQuery, IReadOnlyCollection<AdminRoleResponse>>
{
    public async Task<IReadOnlyCollection<AdminRoleResponse>> Handle(
        GetAdminRolesQuery request,
        CancellationToken cancellationToken) =>
        (await repository.GetRolesAsync(cancellationToken))
        .Select(x => new AdminRoleResponse(x.RoleId, x.Name, x.Description))
        .ToArray();
}
