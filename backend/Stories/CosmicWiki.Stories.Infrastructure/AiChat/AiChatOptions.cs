namespace CosmicWiki.Stories.Infrastructure.AiChat;

public sealed class AiChatOptions
{
    public const string SectionName = "AiChat";

    public string OllamaBaseUrl { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "qwen2.5:1.5b";
    public string ProductApiBaseUrl { get; init; } = "http://localhost:5000/";
}
