using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Messaging.Contracts.Product;

public sealed record ProductUpdatedIntegrationEvent(
    Guid EventId,
    int Version,
    DateTime OccurredAtUtc,
    Guid AggregateId,
    Guid ProductId,
    string SKU,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl
) : IIntegrationEvent;
