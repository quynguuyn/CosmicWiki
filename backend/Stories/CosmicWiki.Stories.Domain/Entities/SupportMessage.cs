namespace CosmicWiki.Stories.Domain.Entities;

public sealed class SupportMessage
{
    private SupportMessage() { }

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public string SenderType { get; private set; } = string.Empty;
    public Guid? SenderUserId { get; private set; }
    public string? SenderDisplayName { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public SupportConversation Conversation { get; private set; } = null!;

    public static SupportMessage Create(
        Guid conversationId,
        string senderType,
        Guid? senderUserId,
        string? senderDisplayName,
        string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        return new SupportMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderType = senderType,
            SenderUserId = senderUserId,
            SenderDisplayName = senderDisplayName,
            Body = body.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
