using CosmicWiki.Auth.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Auth.Application.Features.AdminAccounts.RemovePermission;

public sealed class RemoveAdminPermissionCommandHandler(IAdminAccountRepository repository)
    : IRequestHandler<RemoveAdminPermissionCommand, bool>
{
    public Task<bool> Handle(
        RemoveAdminPermissionCommand request,
        CancellationToken cancellationToken) =>
        repository.RemovePermissionAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);
}
