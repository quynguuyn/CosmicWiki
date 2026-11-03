using CosmicWiki.Stories.Domain.Entities;

namespace CosmicWiki.Stories.Application.Abstractions.Persistence;

public interface ISupportChatRepository
{
    Task<SupportConversation?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken);

    Task<SupportConversation?> GetLatestActiveConversationForCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken);

    Task<SupportConversation?> GetActiveConversationForGuestAsync(
        string guestTokenHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupportMessage>> GetMessagesAsync(
        Guid conversationId,
        DateTime? afterUtc,
        int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupportConversation>> GetAgentConversationCandidatesAsync(
        Guid supportUserId,
        int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupportConversation>> GetQueuedConversationsAsync(
        int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupportMessage>> GetLatestMessagesAsync(
        IReadOnlyCollection<Guid> conversationIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupportAgentPresence>> GetActiveAgentsAsync(
        DateTime activeSinceUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> GetOpenConversationCountsAsync(
        CancellationToken cancellationToken);

    Task<SupportAgentPresence?> GetAgentPresenceAsync(
        Guid supportUserId,
        CancellationToken cancellationToken);

    Task AddConversationAsync(
        SupportConversation conversation,
        CancellationToken cancellationToken);

    Task AddMessageAsync(SupportMessage message, CancellationToken cancellationToken);

    Task AddAgentPresenceAsync(
        SupportAgentPresence presence,
        CancellationToken cancellationToken);

    Task<bool> TryAssignQueuedConversationAsync(
        Guid conversationId,
        Guid supportUserId,
        string? supportDisplayName,
        DateTime assignedAtUtc,
        int maxOpenConversations,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
