using System.Text.Json;
using CosmicWiki.Messaging.Abstractions;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Outbox;

public sealed class OrderingOutboxWriter(OrderingDbContext dbContext) : IOutboxWriter
{
    public void Add<T>(T integrationEvent, string topic) where T : IIntegrationEvent
    {
        dbContext.OutboxMessages.Add(OutboxMessage.Create(
            integrationEvent.EventId,
            integrationEvent.AggregateId,
            topic,
            typeof(T).AssemblyQualifiedName!,
            JsonSerializer.Serialize(integrationEvent),
            integrationEvent.OccurredAtUtc));
    }
}
