using System.Text.Json;
using Confluent.Kafka;
using CosmicWiki.Discovery.Domain.Entities;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Catalog;
using CosmicWiki.Messaging.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Discovery.Infrastructure.Messaging;

public sealed class StarSearchDocumentConsumer : BackgroundService
{
    private const string SourceType = "star";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaConsumerFactory _consumerFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<StarSearchDocumentConsumer> _logger;

    public StarSearchDocumentConsumer(IServiceScopeFactory scopeFactory,
        KafkaConsumerFactory consumerFactory, IEventBus eventBus,
        ILogger<StarSearchDocumentConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumerFactory = consumerFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = _consumerFactory.Create(
            "cosmicwiki.discovery.star-indexer");

        consumer.Subscribe(KafkaTopics.CatalogStarCreated);
        _logger.LogInformation("Star search-document consumer started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                try
                {
                    await RetryAsync(() => HandleAsync(result, stoppingToken), stoppingToken);
                    consumer.Commit(result);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Moving failed Kafka message from {Topic} to its DLQ.", result.Topic);
                    var eventId = Guid.TryParse(result.Message.Key, out var parsed) ? parsed : Guid.NewGuid();
                    await _eventBus.PublishRawAsync(eventId, result.Message.Value, $"{result.Topic}.dlq",
                        stoppingToken, result.Message.Key);
                    consumer.Commit(result);
                }
            }

        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Star search-document consumer stopped.");
        }
    }

    private async Task HandleAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var integrationEvent = JsonSerializer.Deserialize<StarCreatedIntegrationEvent>(result.Message.Value)
            ?? throw new InvalidOperationException("Could not deserialize StarCreatedIntegrationEvent.");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.DiscoveryDbContext>();
        var document = await db.SearchDocuments.SingleOrDefaultAsync(
            x => x.SourceType == SourceType && x.SourceId == integrationEvent.StarId, cancellationToken);

        if (document is null)
        {
            db.SearchDocuments.Add(SearchDocument.Create(integrationEvent.StarId, SourceType,
                integrationEvent.Name, integrationEvent.Description, integrationEvent.ImageUrl,
                integrationEvent.OccurredAtUtc));
        }
        else
        {
            document.Update(integrationEvent.Name, integrationEvent.Description,
                integrationEvent.ImageUrl, integrationEvent.OccurredAtUtc);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task RetryAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try { await action(); return; }
            catch when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), cancellationToken);
            }
        }
    }
}
