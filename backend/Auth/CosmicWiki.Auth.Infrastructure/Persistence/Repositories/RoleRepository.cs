using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _dbContext;

    public RoleRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .SingleOrDefaultAsync(
                x => x.Name == name,
                cancellationToken);
    }
}
