using CosmicWiki.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace CosmicWiki.Payments.Infrastructure.Persistence.Outbox;

public sealed class PaymentOutboxPublisher(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<PaymentOutboxPublisher> logger) : BackgroundService
{
    private static readonly Meter Meter = new("CosmicWiki.Payments.Outbox");
    private static readonly Counter<long> Published = Meter.CreateCounter<long>("outbox.published");
    private static readonly Counter<long> Failed = Meter.CreateCounter<long>("outbox.failed");
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var messages = await db.OutboxMessages
                    .Where(x => x.PublishedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        await eventBus.PublishRawAsync(
                            message.EventId,
                            message.Payload,
                            message.Topic,
                            stoppingToken,
                            message.AggregateId.ToString());
                        message.MarkPublished();
                        Published.Add(1);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        message.MarkFailed(ex.Message);
                        Failed.Add(1);
                        logger.LogError(ex, "Failed to publish payment outbox message {MessageId}.", message.Id);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
                await Task.Delay(
                    messages.Count == 0 ? TimeSpan.FromSeconds(2) : TimeSpan.FromMilliseconds(100),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Payment outbox publisher failed.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
