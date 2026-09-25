using CosmicWiki.Auth.Domain.Entities;

namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface ITokenService
{
    string GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> permissions);
}