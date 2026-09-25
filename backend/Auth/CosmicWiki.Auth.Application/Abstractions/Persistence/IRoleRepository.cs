using CosmicWiki.Auth.Domain.Entities;

namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}