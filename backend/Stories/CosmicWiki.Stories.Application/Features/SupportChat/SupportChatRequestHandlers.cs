using System.Security.Cryptography;
using System.Text;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Domain.Entities;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.SupportChat;

public sealed class OpenSupportConversationHandler(ISupportChatRepository repository)
    : IRequestHandler<OpenSupportConversationCommand, SupportChatOpenResponse>
{
    public async Task<SupportChatOpenResponse> Handle(
        OpenSupportConversationCommand request,
        CancellationToken cancellationToken)
    {
        SupportConversation? conversation = null;
        if (!string.IsNullOrWhiteSpace(request.GuestAccessToken))
        {
            conversation = await repository.GetActiveConversationForGuestAsync(
                SupportChatApplication.HashToken(request.GuestAccessToken),
                cancellationToken);
        }

        if (request.CustomerUserId.HasValue)
        {
            var userConversation =
                await repository.GetLatestActiveConversationForCustomerAsync(
                    request.CustomerUserId.Value,
                    cancellationToken);
            if (userConversation is not null)
                conversation = userConversation;
            else if (conversation is not null)
                conversation.LinkCustomer(
                    request.CustomerUserId.Value,
                    request.CustomerDisplayName);

            conversation?.SetCustomerDisplayName(request.CustomerDisplayName);
        }

        string? guestAccessToken = null;
        if (conversation is null)
        {
            string? guestTokenHash = null;
            if (!request.CustomerUserId.HasValue)
            {
                guestAccessToken = SupportChatApplication.CreateGuestToken();
                guestTokenHash = SupportChatApplication.HashToken(guestAccessToken);
            }

            conversation = SupportConversation.Create(
                request.CustomerUserId,
                guestTokenHash,
                request.CustomerDisplayName);
            await repository.AddConversationAsync(conversation, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        await SupportChatApplication.AssignQueuedConversationsAsync(
            repository, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new(
            SupportChatApplication.ToConversationResponse(conversation, []),
            guestAccessToken);
    }
}

public sealed class GetCustomerSupportConversationHandler(ISupportChatRepository repository)
    : IRequestHandler<GetCustomerSupportConversationQuery, SupportChatConversationResponse?>
{
    public async Task<SupportChatConversationResponse?> Handle(
        GetCustomerSupportConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        if (!SupportChatApplication.CanAccessCustomerConversation(
                conversation, request.CustomerUserId, request.GuestAccessToken))
            return null;

        var messages = await repository.GetMessagesAsync(
            request.ConversationId, request.AfterUtc, 100, cancellationToken);
        return SupportChatApplication.ToConversationResponse(
            conversation!,
            messages.OrderBy(message => message.CreatedAtUtc)
                .Select(SupportChatApplication.ToMessageResponse)
                .ToArray());
    }
}

public sealed class SendCustomerSupportMessageHandler(ISupportChatRepository repository)
    : IRequestHandler<SendCustomerSupportMessageCommand, SupportChatMessageResponse?>
{
    public async Task<SupportChatMessageResponse?> Handle(
        SendCustomerSupportMessageCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        if (!SupportChatApplication.CanAccessCustomerConversation(
                conversation, request.CustomerUserId, request.GuestAccessToken) ||
            conversation!.Status == SupportConversation.Statuses.Closed)
            return null;

        var message = SupportMessage.Create(
            conversation.Id,
            "Customer",
            request.CustomerUserId,
            request.CustomerDisplayName,
            SupportChatApplication.ValidateBody(request.Body));
        conversation.AddMessage(message);
        await repository.AddMessageAsync(message, cancellationToken);
        await SupportChatApplication.AssignQueuedConversationsAsync(
            repository, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return SupportChatApplication.ToMessageResponse(message);
    }
}

public sealed class CloseCustomerSupportConversationHandler(ISupportChatRepository repository)
    : IRequestHandler<CloseCustomerSupportConversationCommand, bool>
{
    public async Task<bool> Handle(
        CloseCustomerSupportConversationCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        if (!SupportChatApplication.CanAccessCustomerConversation(
                conversation, request.CustomerUserId, request.GuestAccessToken))
            return false;

        conversation!.Close();
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class GetAgentSupportConversationsHandler(ISupportChatRepository repository)
    : IRequestHandler<GetAgentSupportConversationsQuery, IReadOnlyCollection<SupportChatQueueItem>>
{
    public async Task<IReadOnlyCollection<SupportChatQueueItem>> Handle(
        GetAgentSupportConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await repository.GetAgentConversationCandidatesAsync(
            request.SupportUserId, 100, cancellationToken);
        var conversationIds = conversations.Select(conversation => conversation.Id).ToArray();
        var latestMessages = await repository.GetLatestMessagesAsync(
            conversationIds, cancellationToken);
        var latestByConversation = latestMessages
            .GroupBy(message => message.ConversationId)
            .ToDictionary(group => group.Key, group => group.First());

        return conversations
            .Where(conversation =>
                conversation.Status != SupportConversation.Statuses.Closed &&
                (conversation.AssignedSupportId is null ||
                 conversation.AssignedSupportId == request.SupportUserId))
            .Select(conversation =>
            {
                latestByConversation.TryGetValue(conversation.Id, out var latest);
                return new SupportChatQueueItem(
                    conversation.Id,
                    conversation.Status,
                    conversation.AssignedSupportId,
                    conversation.CustomerDisplayName,
                    conversation.CreatedAtUtc,
                    conversation.LastMessageAtUtc,
                    latest?.Body,
                    latest?.SenderType);
            })
            .ToArray();
    }
}

public sealed class SetSupportAgentPresenceHandler(ISupportChatRepository repository)
    : IRequestHandler<SetSupportAgentPresenceCommand>
{
    private static readonly TimeSpan PresenceLifetime = TimeSpan.FromSeconds(45);

    public async Task Handle(
        SetSupportAgentPresenceCommand request,
        CancellationToken cancellationToken)
    {
        var presence = await repository.GetAgentPresenceAsync(
            request.SupportUserId, cancellationToken);
        if (presence is null)
        {
            presence = SupportAgentPresence.Create(
                request.SupportUserId,
                request.SupportDisplayName,
                request.AcceptingChats);
            await repository.AddAgentPresenceAsync(presence, cancellationToken);
        }
        else
        {
            presence.Heartbeat(
                request.SupportDisplayName,
                request.AcceptingChats);
        }

        await repository.SaveChangesAsync(cancellationToken);
        if (request.AcceptingChats)
            await SupportChatApplication.AssignQueuedConversationsAsync(
                repository, cancellationToken, PresenceLifetime);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ClaimSupportConversationHandler(ISupportChatRepository repository)
    : IRequestHandler<ClaimSupportConversationCommand, bool>
{
    public async Task<bool> Handle(
        ClaimSupportConversationCommand request,
        CancellationToken cancellationToken)
    {
        var presence = await repository.GetAgentPresenceAsync(
            request.SupportUserId, cancellationToken);
        if (presence is null)
        {
            await repository.AddAgentPresenceAsync(
                SupportAgentPresence.Create(
                    request.SupportUserId,
                    request.SupportDisplayName,
                    false),
                cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }

        var claimed = await repository.TryAssignQueuedConversationAsync(
            request.ConversationId,
            request.SupportUserId,
            request.SupportDisplayName,
            DateTime.UtcNow,
            SupportChatApplication.MaxAssignedOpenChats,
            cancellationToken);
        if (claimed)
            return true;

        var existing = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        return existing?.AssignedSupportId == request.SupportUserId &&
               existing.Status == SupportConversation.Statuses.Open;
    }
}

public sealed class GetAgentSupportConversationHandler(ISupportChatRepository repository)
    : IRequestHandler<GetAgentSupportConversationQuery, SupportChatConversationResponse?>
{
    public async Task<SupportChatConversationResponse?> Handle(
        GetAgentSupportConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        if (conversation is null ||
            conversation.Status == SupportConversation.Statuses.Closed ||
            (conversation.AssignedSupportId.HasValue &&
             conversation.AssignedSupportId != request.SupportUserId))
            return null;

        var messages = await repository.GetMessagesAsync(
            request.ConversationId, null, 100, cancellationToken);
        return SupportChatApplication.ToConversationResponse(
            conversation,
            messages.OrderBy(message => message.CreatedAtUtc)
                .Select(SupportChatApplication.ToMessageResponse)
                .ToArray());
    }
}

public sealed class SendAgentSupportMessageHandler(ISupportChatRepository repository)
    : IRequestHandler<SendAgentSupportMessageCommand, SupportChatMessageResponse?>
{
    public async Task<SupportChatMessageResponse?> Handle(
        SendAgentSupportMessageCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        if (conversation is null ||
            conversation.AssignedSupportId != request.SupportUserId ||
            conversation.Status != SupportConversation.Statuses.Open)
            return null;

        var message = SupportMessage.Create(
            conversation.Id,
            "Support",
            request.SupportUserId,
            request.SupportDisplayName,
            SupportChatApplication.ValidateBody(request.Body));
        conversation.AddMessage(message);
        await repository.AddMessageAsync(message, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return SupportChatApplication.ToMessageResponse(message);
    }
}

public sealed class CloseAgentSupportConversationHandler(ISupportChatRepository repository)
    : IRequestHandler<CloseAgentSupportConversationCommand, bool>
{
    public async Task<bool> Handle(
        CloseAgentSupportConversationCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetConversationAsync(
            request.ConversationId, cancellationToken);
        if (conversation is null ||
            conversation.AssignedSupportId != request.SupportUserId ||
            conversation.Status != SupportConversation.Statuses.Open)
            return false;

        conversation.Close();
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

internal static class SupportChatApplication
{
    internal const int MaxAssignedOpenChats = 5;
    private const int MaxMessageLength = 4000;
    private static readonly TimeSpan AgentPresenceLifetime = TimeSpan.FromSeconds(45);

    internal static async Task AssignQueuedConversationsAsync(
        ISupportChatRepository repository,
        CancellationToken cancellationToken,
        TimeSpan? presenceLifetime = null)
    {
        var now = DateTime.UtcNow;
        var agents = await repository.GetActiveAgentsAsync(
            now - (presenceLifetime ?? AgentPresenceLifetime), cancellationToken);
        if (agents.Count == 0)
            return;

        var openCounts = new Dictionary<Guid, int>(
            await repository.GetOpenConversationCountsAsync(cancellationToken));
        var queued = await repository.GetQueuedConversationsAsync(100, cancellationToken);
        foreach (var conversation in queued)
        {
            var attemptedAgents = new HashSet<Guid>();
            while (true)
            {
                var agent = agents
                    .Where(candidate =>
                        !attemptedAgents.Contains(candidate.UserId) &&
                        openCounts.GetValueOrDefault(candidate.UserId) <
                        MaxAssignedOpenChats)
                    .OrderBy(candidate =>
                        openCounts.GetValueOrDefault(candidate.UserId))
                    .ThenBy(candidate => candidate.LastAssignedAtUtc)
                    .ThenBy(candidate => candidate.UserId)
                    .FirstOrDefault();
                if (agent is null)
                    break;

                attemptedAgents.Add(agent.UserId);
                var assigned = await repository.TryAssignQueuedConversationAsync(
                    conversation.Id,
                    agent.UserId,
                    agent.DisplayName,
                    DateTime.UtcNow,
                    MaxAssignedOpenChats,
                    cancellationToken);
                if (assigned)
                {
                    openCounts[agent.UserId] =
                        openCounts.GetValueOrDefault(agent.UserId) + 1;
                    break;
                }

                openCounts = new Dictionary<Guid, int>(
                    await repository.GetOpenConversationCountsAsync(cancellationToken));
            }
        }
    }

    internal static bool CanAccessCustomerConversation(
        SupportConversation? conversation,
        Guid? customerUserId,
        string? guestAccessToken)
    {
        if (conversation is null)
            return false;
        if (customerUserId.HasValue)
            return conversation.CustomerUserId == customerUserId;
        return !string.IsNullOrWhiteSpace(guestAccessToken) &&
               conversation.GuestTokenHash == HashToken(guestAccessToken);
    }

    internal static string ValidateBody(string body)
    {
        var trimmed = body?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Message cannot be empty.", nameof(body));
        if (trimmed.Length > MaxMessageLength)
            throw new ArgumentException(
                $"Message cannot exceed {MaxMessageLength} characters.", nameof(body));
        return trimmed;
    }

    internal static string CreateGuestToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return token.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    internal static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    internal static SupportChatMessageResponse ToMessageResponse(SupportMessage message) =>
        new(
            message.Id,
            message.SenderType,
            message.SenderUserId,
            message.SenderDisplayName,
            message.Body,
            message.CreatedAtUtc);

    internal static SupportChatConversationResponse ToConversationResponse(
        SupportConversation conversation,
        IReadOnlyCollection<SupportChatMessageResponse> messages) =>
        new(
            conversation.Id,
            conversation.Status,
            conversation.AssignedSupportId,
            conversation.CustomerDisplayName,
            conversation.AssignedSupportDisplayName,
            conversation.CreatedAtUtc,
            conversation.UpdatedAtUtc,
            conversation.LastMessageAtUtc,
            messages);
}
