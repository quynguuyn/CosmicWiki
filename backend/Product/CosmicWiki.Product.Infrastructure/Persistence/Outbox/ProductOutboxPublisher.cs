using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Product.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Product.Infrastructure.Persistence.Outbox;

public sealed class ProductOutboxPublisher : BackgroundService
{
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProductOutboxPublisher> _logger;

    public ProductOutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IEventBus eventBus,
        ILogger<ProductOutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Product outbox publisher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published =
                    await PublishBatchAsync(
                        stoppingToken);

                if (published == 0)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(2),
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error while publishing product outbox messages.");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "Product outbox publisher stopped.");
    }

    private async Task<int> PublishBatchAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ProductDbContext>();

        var messages =
            await dbContext.OutboxMessages
                .Where(x => x.PublishedAtUtc == null)
                .OrderBy(x => x.OccurredAtUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return 0;
        }

        var publishedCount = 0;

        foreach (var message in messages)
        {
            try
            {
                await _eventBus.PublishRawAsync(
                    message.EventId,
                    message.Payload,
                    message.Topic,
                    cancellationToken,
                    message.AggregateId.ToString());

                message.MarkPublished(
                    DateTime.UtcNow);

                publishedCount++;
            }
            catch (Exception exception)
            {
                message.MarkFailed(
                    exception.Message);

                _logger.LogError(
                    exception,
                    "Failed to publish outbox message {MessageId} to {Topic}.",
                    message.Id,
                    message.Topic);
            }
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return publishedCount;
    }
}