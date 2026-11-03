using System.Text.Json;
using Confluent.Kafka;
using CosmicWiki.Discovery.Domain.Entities;
using CosmicWiki.Discovery.Infrastructure.Persistence;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Product;
using CosmicWiki.Messaging.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Discovery.Infrastructure.Messaging;

public sealed class ProductSearchDocumentConsumer : BackgroundService
{
    private const string ProductSourceType = SearchDocumentSourceType.Product;
    private const string ConsumerGroupId = "cosmicwiki.discovery.product-indexer";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaConsumerFactory _consumerFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProductSearchDocumentConsumer> _logger;

    public ProductSearchDocumentConsumer(
        IServiceScopeFactory scopeFactory,
        KafkaConsumerFactory consumerFactory,
        IEventBus eventBus,
        ILogger<ProductSearchDocumentConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumerFactory = consumerFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = _consumerFactory.Create(ConsumerGroupId);

        consumer.Subscribe([
            KafkaTopics.ProductCreated,
            KafkaTopics.ProductUpdated,
            KafkaTopics.ProductDeleted
        ]);

        _logger.LogInformation("Product search-document consumer started.");

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
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Product search-document consumer stopped.");
        }
    }

    private async Task HandleAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscoveryDbContext>();

        switch (result.Topic)
        {
            case KafkaTopics.ProductCreated:
                var created = Deserialize<ProductCreatedIntegrationEvent>(result.Message.Value);
                await SearchDocumentUpsert.ExecuteAsync(
                    dbContext,
                    created.ProductId,
                    ProductSourceType,
                    created.Name,
                    created.Description,
                    created.ImageUrl,
                    created.OccurredAtUtc,
                    catalog: null,
                    catalogValue: null,
                    cancellationToken: cancellationToken);
                break;
            case KafkaTopics.ProductUpdated:
                var updated = Deserialize<ProductUpdatedIntegrationEvent>(result.Message.Value);
                await SearchDocumentUpsert.ExecuteAsync(
                    dbContext,
                    updated.ProductId,
                    ProductSourceType,
                    updated.Name,
                    updated.Description,
                    updated.ImageUrl,
                    updated.OccurredAtUtc,
                    catalog: null,
                    catalogValue: null,
                    cancellationToken: cancellationToken);
                break;
            case KafkaTopics.ProductDeleted:
                var deleted = Deserialize<ProductDeletedIntegrationEvent>(result.Message.Value);
                var document = await dbContext.SearchDocuments.SingleOrDefaultAsync(
                    x => x.SourceType == ProductSourceType && x.SourceId == deleted.ProductId,
                    cancellationToken);

                if (document is not null)
                {
                    dbContext.SearchDocuments.Remove(document);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                break;
        }
    }

    private static T Deserialize<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload)
            ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name}.");
    }
}
