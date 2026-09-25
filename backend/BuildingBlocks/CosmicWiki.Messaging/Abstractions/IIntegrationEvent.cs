namespace CosmicWiki.Messaging.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    int Version { get; }
    Guid AggregateId { get; }
    DateTime OccurredAtUtc { get; }
}