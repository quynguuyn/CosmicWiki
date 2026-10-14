using System.Text.Json;
using Confluent.Kafka;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Ordering.Application.Events;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CosmicWiki.Ordering.Infrastructure.Messaging;

public sealed class OrderCreatedNotificationConsumer(
    KafkaConsumerFactory consumerFactory,
    IOrderEmailSender emailSender,
    ILogger<OrderCreatedNotificationConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give Kafka a moment to start
        await Task.Yield();

        using var consumer = consumerFactory.Create("cosmicwiki.ordering.notification");
        consumer.Subscribe("cosmicwiki.ordering");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Order created notification consumer failed to consume a message.");
                    continue;
                }

                try
                {
                    var integrationEvent = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(
                        result.Message.Value,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    if (integrationEvent is not null)
                    {
                        await emailSender.SendOrderCreatedAsync(
                            integrationEvent,
                            stoppingToken);
                    }

                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException && stoppingToken.IsCancellationRequested)
                        throw;

                    logger.LogError(
                        ex,
                        "Failed to process order created notification message at {TopicPartitionOffset}.",
                        result.TopicPartitionOffset);
                    
                    // We just commit anyway for this dummy notification consumer to avoid poison pills
                    consumer.Commit(result);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }
}
