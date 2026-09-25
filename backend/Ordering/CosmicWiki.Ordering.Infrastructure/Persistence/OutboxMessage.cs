namespace CosmicWiki.Ordering.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    private OutboxMessage() { }
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

    public static OutboxMessage Create(Guid eventId, Guid aggregateId, string topic, string type, string payload, DateTime occurredAtUtc) =>
        new() { Id = Guid.NewGuid(), EventId = eventId, AggregateId = aggregateId, Topic = topic, Type = type, Payload = payload, OccurredAtUtc = occurredAtUtc };
    public void MarkPublished() { PublishedAtUtc = DateTime.UtcNow; Error = null; }
    public void MarkFailed(string error) { Attempts++; Error = error; }
}
