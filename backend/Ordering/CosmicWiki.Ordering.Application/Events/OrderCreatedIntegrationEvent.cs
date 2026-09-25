using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Ordering.Application.Events;

public sealed record OrderCreatedIntegrationEvent(
    Guid EventId,
    int Version,
    DateTime OccurredAtUtc,
    Guid AggregateId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency) : IIntegrationEvent;
