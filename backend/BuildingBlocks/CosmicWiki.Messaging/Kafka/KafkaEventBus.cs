using System.Text.Json;
using System.Text;
using Confluent.Kafka;
using CosmicWiki.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Messaging.Kafka;

public sealed class KafkaEventBus : IEventBus
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;

    public KafkaEventBus(
        IProducer<string, string> producer,
        ILogger<KafkaEventBus> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task PublishAsync<T>(
        T integrationEvent,
        string topic,
        CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        var payload = JsonSerializer.Serialize(
            integrationEvent);

        await PublishInternalAsync(
            integrationEvent.EventId,
            payload,
            topic,
            cancellationToken,
            integrationEvent.AggregateId.ToString());
    }

    public async Task PublishRawAsync(
        Guid eventId,
        string payload,
        string topic,
        CancellationToken cancellationToken = default,
        string? partitionKey = null)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Event payload is required.", nameof(payload));

        await PublishInternalAsync(
            eventId,
            payload,
            topic,
            cancellationToken,
            partitionKey);
    }

    private async Task PublishInternalAsync(
        Guid eventId,
        string payload,
        string topic,
        CancellationToken cancellationToken,
        string? partitionKey)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Kafka topic is required.", nameof(topic));

        var message = new Message<string, string>
        {
            Key = partitionKey ?? eventId.ToString(),
            Value = payload,
            Headers = new Headers
            {
                { KafkaHeaders.EventId, Encoding.UTF8.GetBytes(eventId.ToString()) },
                { KafkaHeaders.ContentType, Encoding.UTF8.GetBytes("application/json") }
            }
        };

        var result = await _producer.ProduceAsync(
            topic,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Published event {EventId} to Kafka topic {Topic}, partition {Partition}, offset {Offset}",
            eventId,
            topic,
            result.Partition,
            result.Offset);
    }
}