namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Username { get; }

    bool IsAuthenticated { get; }
}