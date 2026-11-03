using MediatR;

namespace CosmicWiki.Stories.Application.Features.SupportChat;

public sealed record OpenSupportConversationCommand(
    Guid? CustomerUserId,
    string? CustomerDisplayName,
    string? GuestAccessToken) : IRequest<SupportChatOpenResponse>;

public sealed record GetCustomerSupportConversationQuery(
    Guid ConversationId,
    Guid? CustomerUserId,
    string? GuestAccessToken,
    DateTime? AfterUtc) : IRequest<SupportChatConversationResponse?>;

public sealed record SendCustomerSupportMessageCommand(
    Guid ConversationId,
    Guid? CustomerUserId,
    string? GuestAccessToken,
    string? CustomerDisplayName,
    string Body) : IRequest<SupportChatMessageResponse?>;

public sealed record CloseCustomerSupportConversationCommand(
    Guid ConversationId,
    Guid? CustomerUserId,
    string? GuestAccessToken) : IRequest<bool>;

public sealed record GetAgentSupportConversationsQuery(
    Guid SupportUserId) : IRequest<IReadOnlyCollection<SupportChatQueueItem>>;

public sealed record SetSupportAgentPresenceCommand(
    Guid SupportUserId,
    string? SupportDisplayName,
    bool AcceptingChats) : IRequest;

public sealed record ClaimSupportConversationCommand(
    Guid ConversationId,
    Guid SupportUserId,
    string? SupportDisplayName) : IRequest<bool>;

public sealed record GetAgentSupportConversationQuery(
    Guid ConversationId,
    Guid SupportUserId) : IRequest<SupportChatConversationResponse?>;

public sealed record SendAgentSupportMessageCommand(
    Guid ConversationId,
    Guid SupportUserId,
    string? SupportDisplayName,
    string Body) : IRequest<SupportChatMessageResponse?>;

public sealed record CloseAgentSupportConversationCommand(
    Guid ConversationId,
    Guid SupportUserId) : IRequest<bool>;
