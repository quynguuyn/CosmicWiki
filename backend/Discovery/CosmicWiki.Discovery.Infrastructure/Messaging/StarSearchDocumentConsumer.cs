using System.Text.Json;
using Confluent.Kafka;
using CosmicWiki.Discovery.Domain.Entities;
using CosmicWiki.Discovery.Infrastructure.Persistence;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Catalog;
using CosmicWiki.Messaging.Kafka;
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
                var result = await KafkaConsumerProcessing.ConsumeWithRetryAsync(
                    consumer,
                    stoppingToken);
                await KafkaConsumerProcessing.ProcessWithRetryAndDeadLetterAsync(
                    consumer,
                    result,
                    () => HandleAsync(result, stoppingToken),
                    _eventBus,
                    _logger,
                    stoppingToken);
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
        string? catalog = null;
        string? catalogValue = null;
        DateTime occurredAtUtc;

        if (result.Topic == KafkaTopics.CatalogStarCreated)
        {
            var integrationEvent =
                JsonSerializer.Deserialize<StarCreatedIntegrationEvent>(
                    result.Message.Value)
                ?? throw new InvalidOperationException(
                    "Could not deserialize StarCreatedIntegrationEvent.");
            sourceId = integrationEvent.StarId;
            sourceType = SearchDocumentSourceType.Star;
            name = integrationEvent.Name;
            description = integrationEvent.Description;
            imageUrl = integrationEvent.ImageUrl;
            catalog = integrationEvent.Catalog;
            catalogValue = integrationEvent.CatalogValue;
            occurredAtUtc = integrationEvent.OccurredAtUtc;
        }
        else
        {
            var integrationEvent =
                JsonSerializer.Deserialize<AstronomicalObjectCreatedIntegrationEvent>(
                    result.Message.Value)
                ?? throw new InvalidOperationException(
                    "Could not deserialize AstronomicalObjectCreatedIntegrationEvent.");
            if (string.Equals(
                integrationEvent.Kind,
                SearchDocumentSourceType.Star,
                StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Ignoring astronomical-object event for Star {StarId}; Star search documents are indexed from Star entity events.",
                    integrationEvent.ObjectId);
                return;
            }

            sourceId = integrationEvent.ObjectId;
            sourceType = integrationEvent.Kind.ToLowerInvariant();
            name = integrationEvent.Name;
            description = integrationEvent.ObjectType;
            imageUrl = integrationEvent.ImageUrl;
            catalog = integrationEvent.Catalog;
            catalogValue = integrationEvent.CatalogValue;
            occurredAtUtc = integrationEvent.OccurredAtUtc;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.DiscoveryDbContext>();
        await SearchDocumentUpsert.ExecuteAsync(
            db,
            sourceId,
            sourceType,
            name,
            description,
            imageUrl,
            occurredAtUtc,
            catalog,
            catalogValue,
            cancellationToken);
    }
}
