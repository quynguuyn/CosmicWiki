using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Auth.Domain.DomainEvents;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Username,
    DateTime OccurredOn) : IDomainEvent;