using System.Text.Json;
using CosmicWiki.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CosmicWiki.Catalog.Infrastructure.Persistence;

public sealed class CatalogOutboxMessage
{
    private CatalogOutboxMessage() { }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid AggregateId { get; private set; }
    public string Topic { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }

    public static CatalogOutboxMessage Create(Guid eventId, Guid aggregateId, string topic, string payload, DateTime occurredAtUtc) =>
        new() { Id = Guid.NewGuid(), EventId = eventId, AggregateId = aggregateId, Topic = topic, Payload = payload, OccurredAtUtc = occurredAtUtc };

    public void MarkPublished() => PublishedAtUtc = DateTime.UtcNow;
}

public sealed class CatalogOutboxWriter(CatalogDbContext dbContext) : IOutboxWriter
{
    public void Add<T>(T integrationEvent, string topic) where T : IIntegrationEvent =>
        dbContext.OutboxMessages.Add(CatalogOutboxMessage.Create(integrationEvent.EventId, integrationEvent.AggregateId, topic,
            JsonSerializer.Serialize(integrationEvent), integrationEvent.OccurredAtUtc));
}

public sealed class CatalogOutboxPublisher(IServiceScopeFactory scopeFactory, IEventBus eventBus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var messages = await db.OutboxMessages.Where(x => x.PublishedAtUtc == null).OrderBy(x => x.OccurredAtUtc).Take(20).ToListAsync(stoppingToken);
            foreach (var message in messages)
            {
                await eventBus.PublishRawAsync(message.EventId, message.Payload, message.Topic, stoppingToken, message.AggregateId.ToString());
                message.MarkPublished();
            }
            if (messages.Count > 0) await db.SaveChangesAsync(stoppingToken);
            else await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
