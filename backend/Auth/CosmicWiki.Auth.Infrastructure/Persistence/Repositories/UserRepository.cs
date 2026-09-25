using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using CosmicWiki.Auth.Domain.ValueObjects;
using CosmicWiki.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Auth.Persistence.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Username == Username.Create(username),
                cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = Username.Create(username);

        return await _dbContext.Users
            .AnyAsync(
                x => x.Username == normalizedUsername,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Email.Create(email);

        return await _dbContext.Users
            .AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken);
    }
}