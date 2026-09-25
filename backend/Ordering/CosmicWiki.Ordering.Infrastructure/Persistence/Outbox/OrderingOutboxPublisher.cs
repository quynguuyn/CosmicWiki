using CosmicWiki.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Outbox;

public sealed class OrderingOutboxPublisher(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<OrderingOutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                var messages = await db.OutboxMessages.Where(x => x.PublishedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc).Take(20).ToListAsync(stoppingToken);
                foreach (var message in messages)
                {
                    try
                    {
                        await eventBus.PublishRawAsync(message.EventId, message.Payload, message.Topic, stoppingToken, message.AggregateId.ToString());
                        message.MarkPublished();
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        message.MarkFailed(ex.Message);
                        logger.LogError(ex, "Failed to publish ordering outbox message {MessageId}.", message.Id);
                    }
                }
                await db.SaveChangesAsync(stoppingToken);
                await Task.Delay(messages.Count == 0 ? TimeSpan.FromSeconds(2) : TimeSpan.FromMilliseconds(100), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ordering outbox publisher failed.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
