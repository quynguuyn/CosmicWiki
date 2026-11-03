using MediatR;

namespace CosmicWiki.Stories.Application.Features.AiChat;

public sealed class AskCosmicAssistantHandler(IAiChatAssistant assistant)
    : IRequestHandler<AskCosmicAssistantCommand, string>
{
    public Task<string> Handle(
        AskCosmicAssistantCommand request,
        CancellationToken cancellationToken) =>
        assistant.ReplyAsync(request.Conversation, cancellationToken);
}
