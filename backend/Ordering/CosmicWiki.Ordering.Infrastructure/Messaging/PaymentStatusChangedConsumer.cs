using System.Text.Json;
using Confluent.Kafka;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Payment;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Payments.Application.Abstractions;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace CosmicWiki.Ordering.Infrastructure.Messaging;

public sealed class PaymentStatusChangedConsumer(
    IServiceScopeFactory scopeFactory,
    KafkaConsumerFactory consumerFactory,
    IOptions<KafkaOptions> options,
    IEventBus eventBus,
    ILogger<PaymentStatusChangedConsumer> logger) : BackgroundService
{
    private static readonly Meter Meter = new("CosmicWiki.Ordering.PaymentConsumer");
    private static readonly Counter<long> Duplicates = Meter.CreateCounter<long>("inbox.duplicates");
    private static readonly Counter<long> Retries = Meter.CreateCounter<long>("inbox.retries");
    private static readonly Counter<long> DeadLetters = Meter.CreateCounter<long>("inbox.dead_letters");
    private static readonly Counter<long> Processed = Meter.CreateCounter<long>("inbox.processed");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = consumerFactory.Create(
            options.Value.PaymentStatusConsumerGroup);
        consumer.Subscribe(KafkaTopics.PaymentStatusChanged);
        await Task.Yield();
        var kafkaOptions = options.Value;
        if (kafkaOptions.PaymentStatusMaxProcessingAttempts < 1)
            throw new InvalidOperationException(
                "Kafka PaymentStatusMaxProcessingAttempts must be at least one.");
        if (kafkaOptions.PaymentStatusRetryBackoffMilliseconds < 0)
            throw new InvalidOperationException(
                "Kafka PaymentStatusRetryBackoffMilliseconds cannot be negative.");

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
                    logger.LogError(ex, "Payment status consumer failed to consume a message.");
                    continue;
                }

                for (var attempt = 1;
                     attempt <= kafkaOptions.PaymentStatusMaxProcessingAttempts;
                     attempt++)
                {
                    try
                    {
                        var integrationEvent = JsonSerializer.Deserialize<PaymentStatusChangedIntegrationEvent>(
                            result.Message.Value,
                            new JsonSerializerOptions(JsonSerializerDefaults.Web))
                            ?? throw new InvalidOperationException("Payment status event payload is empty.");

                        using var scope = scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                        if (await db.InboxMessages.AnyAsync(
                                x => x.EventId == integrationEvent.EventId,
                                stoppingToken))
                        {
                            Duplicates.Add(1);
                            consumer.Commit(result);
                            break;
                        }

                        await using var transaction =
                            await db.Database.BeginTransactionAsync(stoppingToken);
                        var updater = scope.ServiceProvider.GetRequiredService<IOrderPaymentStatusUpdater>();
                        var applied = integrationEvent.Status.ToLowerInvariant() switch
                        {
                            "succeeded" => await updater.MarkPaidAsync(integrationEvent.OrderId, stoppingToken),
                            "failed" => await MarkFailedAsync(updater, integrationEvent.OrderId, stoppingToken),
                            _ => throw new InvalidOperationException(
                                $"Unsupported payment status '{integrationEvent.Status}'.")
                        };

                        if (!applied)
                            throw new InvalidOperationException(
                                $"Payment status event {integrationEvent.EventId} could not be applied.");

                        db.InboxMessages.Add(new InboxMessage(integrationEvent.EventId));
                        await db.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken);
                        consumer.Commit(result);
                        Processed.Add(1);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException &&
                            stoppingToken.IsCancellationRequested)
                            throw;

                        if (IsPermanentFailure(ex) ||
                            attempt == kafkaOptions.PaymentStatusMaxProcessingAttempts)
                        {
                            await PublishDeadLetterAsync(
                                eventBus,
                                result,
                                ex,
                                attempt,
                                stoppingToken);
                            consumer.Commit(result);
                            DeadLetters.Add(1);
                            logger.LogError(
                                ex,
                                "Moved payment status message at {TopicPartitionOffset} to dead-letter topic after {Attempts} attempt(s).",
                                result.TopicPartitionOffset,
                                attempt);
                            break;
                        }

                        Retries.Add(1);
                        logger.LogWarning(
                            ex,
                            "Failed to process payment status message at {TopicPartitionOffset}; retry {Attempt}/{MaxAttempts}.",
                            result.TopicPartitionOffset,
                            attempt,
                            kafkaOptions.PaymentStatusMaxProcessingAttempts);
                        await Task.Delay(
                            TimeSpan.FromMilliseconds(
                                (long)kafkaOptions.PaymentStatusRetryBackoffMilliseconds * attempt),
                            stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            consumer.Close();
        }
    }

    private static async Task<bool> MarkFailedAsync(
        IOrderPaymentStatusUpdater updater,
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await updater.MarkPaymentFailedAsync(orderId, cancellationToken);
        return true;
    }

    private static bool IsPermanentFailure(Exception exception) =>
        exception is JsonException ||
        exception is InvalidOperationException;

    private static Task PublishDeadLetterAsync(
        IEventBus eventBus,
        ConsumeResult<string, string> result,
        Exception exception,
        int attempts,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(
            new DeadLetterMessage(
                result.Topic,
                result.Partition.Value,
                result.Offset.Value,
                result.Message.Key,
                result.Message.Value,
                exception.GetType().Name,
                exception.Message,
                attempts,
                DateTime.UtcNow),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return eventBus.PublishRawAsync(
            Guid.NewGuid(),
            payload,
            KafkaTopics.PaymentStatusChangedDeadLetter,
            cancellationToken,
            result.Message.Key);
    }

    private sealed record DeadLetterMessage(
        string Topic,
        int Partition,
        long Offset,
        string? Key,
        string? Payload,
        string ErrorType,
        string Error,
        int Attempts,
        DateTime FailedAtUtc);
}
