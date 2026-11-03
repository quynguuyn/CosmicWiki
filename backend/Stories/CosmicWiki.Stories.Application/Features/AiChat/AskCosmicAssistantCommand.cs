using MediatR;

namespace CosmicWiki.Stories.Application.Features.AiChat;

public sealed record AskCosmicAssistantCommand(
    IReadOnlyCollection<AiChatTurn> Conversation) : IRequest<string>;
