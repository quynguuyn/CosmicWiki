using CosmicWiki.Messaging.Kafka;

namespace CosmicWiki.IntegrationTests;

public sealed class KafkaEventBusValidationTests
{
    [Fact]
    public async Task PublishRaw_rejects_empty_event_id()
    {
        var bus = new KafkaEventBus(
            null!,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<KafkaEventBus>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            bus.PublishRawAsync(Guid.Empty, "{}", "topic"));
    }

    [Fact]
    public async Task PublishRaw_rejects_empty_payload()
    {
        var bus = new KafkaEventBus(
            null!,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<KafkaEventBus>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            bus.PublishRawAsync(Guid.NewGuid(), " ", "topic"));
    }

    [Fact]
    public async Task PublishRaw_rejects_empty_topic()
    {
        var bus = new KafkaEventBus(
            null!,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<KafkaEventBus>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            bus.PublishRawAsync(Guid.NewGuid(), "{}", " "));
    }
}
