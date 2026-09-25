using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Messaging.Contracts.Payment;

public sealed record PaymentStatusChangedIntegrationEvent(
    Guid EventId,
    int Version,
    DateTime OccurredAtUtc,
    Guid AggregateId,
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    string Status) : IIntegrationEvent;
