namespace CosmicWiki.Stories.Application.Features.SupportChat;

public sealed record SupportChatMessageResponse(
    Guid Id,
    string SenderType,
    Guid? SenderUserId,
    string? SenderDisplayName,
    string Body,
    DateTime CreatedAtUtc);

public sealed record SupportChatConversationResponse(
    Guid Id,
    string Status,
    Guid? AssignedSupportId,
    string? CustomerDisplayName,
    string? AssignedSupportDisplayName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? LastMessageAtUtc,
    IReadOnlyCollection<SupportChatMessageResponse> Messages);

public sealed record SupportChatOpenResponse(
    SupportChatConversationResponse Conversation,
    string? GuestAccessToken);

public sealed record SupportChatQueueItem(
    Guid Id,
    string Status,
    Guid? AssignedSupportId,
    string? CustomerDisplayName,
    DateTime CreatedAtUtc,
    DateTime? LastMessageAtUtc,
    string? LastMessage,
    string? LastSenderType);
