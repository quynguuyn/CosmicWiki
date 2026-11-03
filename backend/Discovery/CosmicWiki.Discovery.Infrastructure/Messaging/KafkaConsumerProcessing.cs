using Confluent.Kafka;
using CosmicWiki.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Discovery.Infrastructure.Messaging;

internal static class KafkaConsumerProcessing
{
    public static async Task<ConsumeResult<string, string>> ConsumeWithRetryAsync(
        IConsumer<string, string> consumer,
        CancellationToken stoppingToken)
    {
        const int maxRetries = 30;
        const int retryDelaySeconds = 2;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return consumer.Consume(stoppingToken);
            }
            catch (ConsumeException exception)
                when (exception.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                if (attempt == maxRetries)
                    throw;

                await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), stoppingToken);
            }
        }

        throw new InvalidOperationException("Failed to consume after max retries");
    }

    public static async Task ProcessWithRetryAndDeadLetterAsync(
        IConsumer<string, string> consumer,
        ConsumeResult<string, string> result,
        Func<Task> handleAsync,
        IEventBus eventBus,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await RetryAsync(handleAsync, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Moving failed Kafka message from {Topic} to its DLQ.",
                result.Topic);

            var eventId = Guid.TryParse(result.Message.Key, out var parsed)
                ? parsed
                : Guid.NewGuid();

            await eventBus.PublishRawAsync(
                eventId,
                result.Message.Value,
                $"{result.Topic}.dlq",
                cancellationToken,
                result.Message.Key);
        }

        consumer.Commit(result);
    }

    private static async Task RetryAsync(
        Func<Task> action,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch when (attempt < 3)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                    cancellationToken);
            }
        }
    }
}
