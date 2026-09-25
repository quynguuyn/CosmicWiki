using CosmicWiki.Auth.Domain.Entities;

namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);
}