namespace CosmicWiki.Stories.Application.Features.AiChat;

public sealed record AiChatTurn(string Role, string Content);

public interface IAiChatAssistant
{
    Task<string> ReplyAsync(
        IReadOnlyCollection<AiChatTurn> conversation,
        CancellationToken cancellationToken);
}

public sealed class AiChatUnavailableException : Exception
{
    public AiChatUnavailableException(bool isConfigured = true)
    {
        IsConfigured = isConfigured;
    }

    public bool IsConfigured { get; }
}
