using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Messaging.Contracts.Catalog;

public sealed record AstronomicalObjectCreatedIntegrationEvent(
    Guid EventId,
    int Version,
    DateTime OccurredAtUtc,
    Guid AggregateId,
    Guid ObjectId,
    string Kind,
    string Name,
    string? ObjectType,
    string? ImageUrl) : IIntegrationEvent;
