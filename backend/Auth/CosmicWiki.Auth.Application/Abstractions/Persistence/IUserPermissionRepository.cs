using System;

namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IUserPermissionRepository
{
    Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}