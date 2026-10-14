using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.AssignPermission;

public sealed class AssignAdminPermissionCommandHandler(IAdminAccountRepository repository)
    : IRequestHandler<AssignAdminPermissionCommand, bool>
{
    public Task<bool> Handle(
        AssignAdminPermissionCommand request,
        CancellationToken cancellationToken) =>
        repository.AssignPermissionAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);
}
