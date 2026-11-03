namespace CosmicWiki.Stories.Domain.Entities;

public sealed class SupportConversation
{
    private SupportConversation() { }

    public Guid Id { get; private set; }
    public Guid? CustomerUserId { get; private set; }
    public string? CustomerDisplayName { get; private set; }
    public string? GuestTokenHash { get; private set; }
    public Guid? AssignedSupportId { get; private set; }
    public string? AssignedSupportDisplayName { get; private set; }
    public string Status { get; private set; } = Statuses.Queued;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? LastMessageAtUtc { get; private set; }
    public ICollection<SupportMessage> Messages { get; private set; } = new List<SupportMessage>();

    public static SupportConversation Create(
        Guid? customerUserId,
        string? guestTokenHash,
        string? customerDisplayName)
    {
        if (customerUserId is null && string.IsNullOrWhiteSpace(guestTokenHash))
            throw new ArgumentException("A customer or guest token is required.");

        var now = DateTime.UtcNow;
        return new SupportConversation
        {
            Id = Guid.NewGuid(),
            CustomerUserId = customerUserId,
            CustomerDisplayName = customerDisplayName,
            GuestTokenHash = guestTokenHash,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void LinkCustomer(Guid userId, string? displayName)
    {
        CustomerUserId = userId;
        CustomerDisplayName = displayName;
        GuestTokenHash = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetCustomerDisplayName(string? displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            CustomerDisplayName = displayName;
    }

    public void AssignSupport(Guid supportUserId, string? displayName)
    {
        AssignedSupportId = supportUserId;
        AssignedSupportDisplayName = displayName;
        Status = Statuses.Open;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddMessage(SupportMessage message)
    {
        Messages.Add(message);
        LastMessageAtUtc = message.CreatedAtUtc;
        UpdatedAtUtc = message.CreatedAtUtc;
    }

    public void Close()
    {
        Status = Statuses.Closed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static class Statuses
    {
        public const string Queued = "Queued";
        public const string Open = "Open";
        public const string Closed = "Closed";
    }
}
