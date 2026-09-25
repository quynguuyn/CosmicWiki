namespace CosmicWiki.Ordering.Infrastructure.Persistence;

public sealed class InboxMessage
{
    private InboxMessage() { }

    public InboxMessage(Guid eventId)
    {
        EventId = eventId;
        ReceivedAtUtc = DateTime.UtcNow;
    }

    public Guid EventId { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
}
