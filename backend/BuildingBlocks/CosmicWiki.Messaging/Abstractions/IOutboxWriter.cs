namespace CosmicWiki.Messaging.Abstractions;

public interface IOutboxWriter
{
    void Add<T>(
        T integrationEvent,
        string topic)
        where T : IIntegrationEvent;
}