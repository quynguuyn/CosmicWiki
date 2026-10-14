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
    private const string ProductSourceType = "product";
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
                // Retry consume until topics exist
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
                    await _eventBus.PublishRawAsync(eventId, result.Message.Value, $"{result.Topic}.dlq", stoppingToken, result.Message.Key);
                    consumer.Commit(result);
                }
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
                await UpsertAsync(
                    new ProductSearchData(created.ProductId, created.Name, created.Description,
                        created.ImageUrl, created.OccurredAtUtc), dbContext, cancellationToken);
                break;
            case KafkaTopics.ProductUpdated:
                var updated = Deserialize<ProductUpdatedIntegrationEvent>(result.Message.Value);
                await UpsertAsync(
                    new ProductSearchData(updated.ProductId, updated.Name, updated.Description,
                        updated.ImageUrl, updated.OccurredAtUtc), dbContext, cancellationToken);
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

    private static async Task UpsertAsync(
        ProductSearchData integrationEvent,
        DiscoveryDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.SearchDocuments.SingleOrDefaultAsync(
            x => x.SourceType == ProductSourceType && x.SourceId == integrationEvent.ProductId,
            cancellationToken);

        if (document is null)
        {
            dbContext.SearchDocuments.Add(SearchDocument.Create(
                integrationEvent.ProductId,
                ProductSourceType,
                integrationEvent.Name,
                integrationEvent.Description,
                integrationEvent.ImageUrl,
                integrationEvent.OccurredAtUtc));
        }
        else
        {
            document.Update(
                integrationEvent.Name,
                integrationEvent.Description,
                integrationEvent.ImageUrl,
                integrationEvent.OccurredAtUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static T Deserialize<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload)
            ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name}.");
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

    private sealed record ProductSearchData(
        Guid ProductId,
        string Name,
        string? Description,
        string? ImageUrl,
        DateTime OccurredAtUtc);
}
