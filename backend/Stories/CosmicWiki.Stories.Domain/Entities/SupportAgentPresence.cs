namespace CosmicWiki.Stories.Domain.Entities;

public sealed class SupportAgentPresence
{
    private SupportAgentPresence() { }

    public Guid UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public bool AcceptingChats { get; private set; }
    public DateTime LastSeenAtUtc { get; private set; }
    public DateTime? LastAssignedAtUtc { get; private set; }

    public static SupportAgentPresence Create(
        Guid userId,
        string? displayName,
        bool acceptingChats)
    {
        var presence = new SupportAgentPresence { UserId = userId };
        presence.Heartbeat(displayName, acceptingChats);
        return presence;
    }

    public void Heartbeat(string? displayName, bool acceptingChats)
    {
        DisplayName = displayName;
        AcceptingChats = acceptingChats;
        LastSeenAtUtc = DateTime.UtcNow;
    }

    public void MarkAssigned() => LastAssignedAtUtc = DateTime.UtcNow;
}
