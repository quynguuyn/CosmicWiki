using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Messaging.Contracts.Product;

public sealed record ProductDeletedIntegrationEvent(
    Guid EventId,
    int Version,
    DateTime OccurredAtUtc,
    Guid AggregateId,
    Guid ProductId
) : IIntegrationEvent;
