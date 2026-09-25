using System.Text.Json;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Infrastructure.Persistence;

namespace CosmicWiki.Product.Infrastructure.Persistence.Outbox;

public sealed class ProductOutboxWriter
    : IOutboxWriter
{
    private readonly ProductDbContext _dbContext;

    public ProductOutboxWriter(
        ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add<T>(
        T integrationEvent,
        string topic)
        where T : IIntegrationEvent
    {
        var payload = JsonSerializer.Serialize(
            integrationEvent);

        var message = OutboxMessage.Create(
            integrationEvent.EventId,
            integrationEvent.AggregateId,
            topic,
            typeof(T).AssemblyQualifiedName!,
            payload,
            integrationEvent.OccurredAtUtc);

        _dbContext.OutboxMessages.Add(message);
    }
}