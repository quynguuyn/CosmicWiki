namespace CosmicWiki.Product.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }
    public Guid AggregateId { get; private set; }

    public string Topic { get; private set; } = null!;

    public string Type { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public int Attempts { get; private set; }

    public string? Error { get; private set; }

    public static OutboxMessage Create(
        Guid eventId,
        Guid aggregateId,
        string topic,
        string type,
        string payload,
        DateTime occurredAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            AggregateId = aggregateId,
            Topic = topic,
            Type = type,
            Payload = payload,
            OccurredAtUtc = occurredAtUtc,
            Attempts = 0
        };
    }

    public void MarkPublished(DateTime publishedAtUtc)
    {
        PublishedAtUtc = publishedAtUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        Error = error;
    }
}