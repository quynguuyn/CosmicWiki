using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRoleRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AnyAsync(
                x =>
                    x.UserId == userId &&
                    x.RoleId == roleId,
                cancellationToken);
    }

    public async Task<UserRole?> GetAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.RoleId == roleId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        await _dbContext.UserRoles.AddAsync(
            userRole,
            cancellationToken);
    }

    public void Delete(UserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        _dbContext.UserRoles.Remove(userRole);
    }
}