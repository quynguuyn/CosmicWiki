using CosmicWiki.Auth.Domain.Entities;

namespace CosmicWiki.Auth.Application.Abstractions.Persistence;


public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default);

    void Delete(UserRole userRole);

    Task<UserRole?> GetAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default);
}