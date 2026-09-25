namespace CosmicWiki.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(
        T integrationEvent,
        string topic,
        CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;

    Task PublishRawAsync(
        Guid eventId,
        string payload,
        string topic,
        CancellationToken cancellationToken = default,
        string? partitionKey = null);
}