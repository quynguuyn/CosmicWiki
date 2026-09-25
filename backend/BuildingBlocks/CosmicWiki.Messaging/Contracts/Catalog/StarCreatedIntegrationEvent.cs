using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Messaging.Contracts.Catalog;

public sealed record StarCreatedIntegrationEvent(
    Guid EventId,
    int Version,
    DateTime OccurredAtUtc,
    Guid AggregateId,
    Guid StarId,
    string Name,
    string? Description,
    string? ImageUrl
) : IIntegrationEvent;