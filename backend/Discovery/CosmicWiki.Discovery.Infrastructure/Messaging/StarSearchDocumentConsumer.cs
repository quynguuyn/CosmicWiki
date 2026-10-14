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

        consumer.Subscribe([
            KafkaTopics.CatalogStarCreated,
            KafkaTopics.CatalogAstronomicalObjectCreated
        ]);
        _logger.LogInformation("Astronomical-object search-document consumer started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Retry consume until topic exists
                var result = await ConsumeWithRetryAsync(consumer, stoppingToken);
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
            _logger.LogInformation("Astronomical-object search-document consumer stopped.");
        }
    }

    private async Task HandleAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        Guid sourceId;
        string sourceType;
        string name;
        string? description;
        string? imageUrl;
        DateTime occurredAtUtc;

        if (result.Topic == KafkaTopics.CatalogStarCreated)
        {
            var integrationEvent =
                JsonSerializer.Deserialize<StarCreatedIntegrationEvent>(
                    result.Message.Value)
                ?? throw new InvalidOperationException(
                    "Could not deserialize StarCreatedIntegrationEvent.");
            sourceId = integrationEvent.StarId;
            sourceType = "star";
            name = integrationEvent.Name;
            description = integrationEvent.Description;
            imageUrl = integrationEvent.ImageUrl;
            occurredAtUtc = integrationEvent.OccurredAtUtc;
        }
        else
        {
            var integrationEvent =
                JsonSerializer.Deserialize<AstronomicalObjectCreatedIntegrationEvent>(
                    result.Message.Value)
                ?? throw new InvalidOperationException(
                    "Could not deserialize AstronomicalObjectCreatedIntegrationEvent.");
            sourceId = integrationEvent.ObjectId;
            sourceType = integrationEvent.Kind.ToLowerInvariant();
            name = integrationEvent.Name;
            description = integrationEvent.ObjectType;
            imageUrl = null;
            occurredAtUtc = integrationEvent.OccurredAtUtc;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.DiscoveryDbContext>();
        var document = await db.SearchDocuments.SingleOrDefaultAsync(
            x => x.SourceType == sourceType && x.SourceId == sourceId, cancellationToken);

        if (document is null)
        {
            db.SearchDocuments.Add(SearchDocument.Create(
                sourceId,
                sourceType,
                name,
                description,
                imageUrl,
                occurredAtUtc));
        }
        else
        {
            document.Update(name, description, imageUrl, occurredAtUtc);
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

    private static async Task<ConsumeResult<string, string>> ConsumeWithRetryAsync(IConsumer<string, string> consumer, CancellationToken stoppingToken)
    {
        const int maxRetries = 30;
        const int retryDelaySeconds = 2;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return consumer.Consume(stoppingToken);
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                if (attempt == maxRetries)
                    throw;

                await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), stoppingToken);
            }
        }
        throw new InvalidOperationException("Failed to consume after max retries");
    }
}
