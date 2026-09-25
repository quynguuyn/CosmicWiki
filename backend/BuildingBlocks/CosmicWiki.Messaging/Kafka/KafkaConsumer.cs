using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Messaging.Kafka;

public sealed class KafkaConsumerFactory(IOptions<KafkaOptions> options)
{
    public IConsumer<string, string> Create(string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentException("Kafka consumer group is required.", nameof(groupId));
        if (string.IsNullOrWhiteSpace(options.Value.BootstrapServers))
            throw new InvalidOperationException(
                "Kafka BootstrapServers is not configured.");

        var config = new ConsumerConfig(
            DependencyInjection.CreateClientConfig(options.Value))
        {
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        return new ConsumerBuilder<string, string>(config).Build();
    }
}