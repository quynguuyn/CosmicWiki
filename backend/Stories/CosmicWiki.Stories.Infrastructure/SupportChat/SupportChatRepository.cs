using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Domain.Entities;
using CosmicWiki.Stories.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Stories.Infrastructure.SupportChat;

public sealed class SupportChatRepository(StoriesDbContext dbContext)
    : ISupportChatRepository
{
    public Task<SupportConversation?> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken) =>
        dbContext.SupportConversations.SingleOrDefaultAsync(
            conversation => conversation.Id == conversationId,
            cancellationToken);

    public Task<SupportConversation?> GetLatestActiveConversationForCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken) =>
        dbContext.SupportConversations
            .Where(conversation =>
                conversation.CustomerUserId == customerUserId &&
                conversation.Status != SupportConversation.Statuses.Closed)
            .OrderByDescending(conversation => conversation.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<SupportConversation?> GetActiveConversationForGuestAsync(
        string guestTokenHash,
        CancellationToken cancellationToken) =>
        dbContext.SupportConversations
            .FirstOrDefaultAsync(
                conversation =>
                    conversation.GuestTokenHash == guestTokenHash &&
                    conversation.Status != SupportConversation.Statuses.Closed,
                cancellationToken);

    public async Task<IReadOnlyList<SupportMessage>> GetMessagesAsync(
        Guid conversationId,
        DateTime? afterUtc,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = dbContext.SupportMessages
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId);
        query = afterUtc.HasValue
            ? query.Where(message => message.CreatedAtUtc > afterUtc.Value)
                .OrderBy(message => message.CreatedAtUtc)
            : query.OrderByDescending(message => message.CreatedAtUtc);
        return await query.Take(limit).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportConversation>> GetAgentConversationCandidatesAsync(
        Guid supportUserId,
        int limit,
        CancellationToken cancellationToken) =>
        await dbContext.SupportConversations
            .AsNoTracking()
            .Where(conversation =>
                conversation.Status != SupportConversation.Statuses.Closed &&
                (conversation.AssignedSupportId == supportUserId ||
                 conversation.AssignedSupportId == null))
            .OrderByDescending(conversation =>
                conversation.LastMessageAtUtc ?? conversation.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupportConversation>> GetQueuedConversationsAsync(
        int limit,
        CancellationToken cancellationToken) =>
        await dbContext.SupportConversations
            .AsNoTracking()
            .Where(conversation =>
                conversation.Status == SupportConversation.Statuses.Queued &&
                conversation.AssignedSupportId == null)
            .OrderBy(conversation => conversation.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupportMessage>> GetLatestMessagesAsync(
        IReadOnlyCollection<Guid> conversationIds,
        CancellationToken cancellationToken)
    {
        if (conversationIds.Count == 0)
            return [];

        return await dbContext.SupportMessages
            .AsNoTracking()
            .Where(message => conversationIds.Contains(message.ConversationId))
            .OrderByDescending(message => message.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportAgentPresence>> GetActiveAgentsAsync(
        DateTime activeSinceUtc,
        CancellationToken cancellationToken) =>
        await dbContext.SupportAgentPresence
            .AsNoTracking()
            .Where(presence =>
                presence.AcceptingChats &&
                presence.LastSeenAtUtc >= activeSinceUtc)
            .OrderBy(presence => presence.LastAssignedAtUtc)
            .ThenBy(presence => presence.UserId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> GetOpenConversationCountsAsync(
        CancellationToken cancellationToken) =>
        await dbContext.SupportConversations
            .Where(conversation =>
                conversation.Status == SupportConversation.Statuses.Open &&
                conversation.AssignedSupportId != null)
            .GroupBy(conversation => conversation.AssignedSupportId!.Value)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.UserId, entry => entry.Count, cancellationToken);

    public Task<SupportAgentPresence?> GetAgentPresenceAsync(
        Guid supportUserId,
        CancellationToken cancellationToken) =>
        dbContext.SupportAgentPresence.SingleOrDefaultAsync(
            presence => presence.UserId == supportUserId,
            cancellationToken);

    public Task AddConversationAsync(
        SupportConversation conversation,
        CancellationToken cancellationToken) =>
        dbContext.SupportConversations.AddAsync(conversation, cancellationToken).AsTask();

    public Task AddMessageAsync(
        SupportMessage message,
        CancellationToken cancellationToken) =>
        dbContext.SupportMessages.AddAsync(message, cancellationToken).AsTask();

    public Task AddAgentPresenceAsync(
        SupportAgentPresence presence,
        CancellationToken cancellationToken) =>
        dbContext.SupportAgentPresence.AddAsync(presence, cancellationToken).AsTask();

    public async Task<bool> TryAssignQueuedConversationAsync(
        Guid conversationId,
        Guid supportUserId,
        string? supportDisplayName,
        DateTime assignedAtUtc,
        int maxOpenConversations,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var lockedPresence = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"COSMICWIKI_STORIES\".\"SUPPORT_AGENT_PRESENCE\" SET \"LAST_SEEN_AT_UTC\" = \"LAST_SEEN_AT_UTC\" WHERE \"USER_ID\" = {supportUserId}",
            cancellationToken);
        if (lockedPresence == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var openCount = await dbContext.SupportConversations
            .CountAsync(
                conversation =>
                    conversation.AssignedSupportId == supportUserId &&
                    conversation.Status == SupportConversation.Statuses.Open,
                cancellationToken);
        if (openCount >= maxOpenConversations)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var updated = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"COSMICWIKI_STORIES\".\"SUPPORT_CONVERSATIONS\" SET \"ASSIGNED_SUPPORT_ID\" = {supportUserId}, \"ASSIGNED_SUPPORT_DISPLAY_NAME\" = {supportDisplayName}, \"STATUS\" = {SupportConversation.Statuses.Open}, \"UPDATED_AT_UTC\" = {assignedAtUtc} WHERE \"ID\" = {conversationId} AND \"ASSIGNED_SUPPORT_ID\" IS NULL AND \"STATUS\" = {SupportConversation.Statuses.Queued}",
            cancellationToken);
        if (updated == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"COSMICWIKI_STORIES\".\"SUPPORT_AGENT_PRESENCE\" SET \"LAST_ASSIGNED_AT_UTC\" = {assignedAtUtc} WHERE \"USER_ID\" = {supportUserId}",
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
