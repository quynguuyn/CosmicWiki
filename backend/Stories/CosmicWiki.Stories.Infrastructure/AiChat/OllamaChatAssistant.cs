using System.Net.Http.Json;
using System.Text.Json;
using CosmicWiki.Stories.Application.Features.AiChat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Stories.Infrastructure.AiChat;

public sealed class OllamaChatAssistant(
    HttpClient httpClient,
    CosmicWikiProductCatalogClient productCatalog,
    IOptions<AiChatOptions> options,
    ILogger<OllamaChatAssistant> logger) : IAiChatAssistant
{
    private const int MaxTurns = 12;
    private const int MaxMessageLength = 4000;
    private readonly AiChatOptions _options = options.Value;

    public async Task<string> ReplyAsync(
        IReadOnlyCollection<AiChatTurn> conversation,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        ValidateConversation(conversation);

        var productContext = await productCatalog.GetProductContextAsync(cancellationToken);
        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = BuildSystemPrompt(productContext)
            }
        };
        messages.AddRange(conversation.Select(turn =>
            (object)new { role = turn.Role, content = turn.Content }));

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/chat",
                new
                {
                    model = _options.Model,
                    messages,
                    stream = false,
                    keep_alive = "10m",
                    options = new { temperature = 0.4, num_predict = 500 }
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Self-hosted Ollama chat returned HTTP {StatusCode}.",
                    (int)response.StatusCode);
                throw new AiChatUnavailableException();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var payload = await JsonDocument.ParseAsync(
                stream, cancellationToken: cancellationToken);
            if (!payload.RootElement.TryGetProperty("message", out var message) ||
                !message.TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(contentElement.GetString()))
            {
                logger.LogWarning("Self-hosted Ollama chat returned an incomplete response.");
                throw new AiChatUnavailableException();
            }

            return contentElement.GetString()!.Trim();
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Self-hosted Ollama chat request failed.");
            throw new AiChatUnavailableException();
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Self-hosted Ollama chat returned an invalid response.");
            throw new AiChatUnavailableException();
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Self-hosted Ollama chat request timed out.");
            throw new AiChatUnavailableException();
        }
    }

    private void ValidateConfiguration()
    {
        if (!Uri.TryCreate(_options.OllamaBaseUrl, UriKind.Absolute, out var endpoint) ||
            endpoint.Scheme is not ("http" or "https") ||
            string.IsNullOrWhiteSpace(_options.Model))
        {
            logger.LogWarning(
                "Self-hosted AI chat is not configured. Set AiChat:OllamaBaseUrl and AiChat:Model.");
            throw new AiChatUnavailableException(isConfigured: false);
        }
    }

    private static string BuildSystemPrompt(string? productContext)
    {
        var prompt =
            "Bạn là trợ lý AI của CosmicWiki, một ứng dụng tiếng Việt giúp khám phá thiên văn, " +
            "đọc các bài viết về vũ trụ, xem nội dung NASA APOD và tìm hiểu sản phẩm trong cửa hàng. " +
            "Trả lời bằng tiếng Việt, ngắn gọn, thân thiện. Không bịa thông tin; nếu chưa biết thì " +
            "nói rõ. Chỉ tư vấn sản phẩm có trong danh sách dữ liệu hiện tại bên dưới, không tự " +
            "đoán tồn kho hoặc giá. Nếu người dùng hỏi ngoài phạm vi CosmicWiki, thiên văn hoặc " +
            "sản phẩm cửa hàng, lịch sự hướng họ về các chủ đề này.";

        return string.IsNullOrWhiteSpace(productContext)
            ? prompt
            : $"{prompt}\n\nDữ liệu sản phẩm đang bán (chỉ dùng làm thông tin tham khảo):\n{productContext}";
    }

    private static void ValidateConversation(IReadOnlyCollection<AiChatTurn> conversation)
    {
        if (conversation.Count is < 1 or > MaxTurns ||
            conversation.Any(turn =>
                turn.Role is not ("user" or "assistant") ||
                string.IsNullOrWhiteSpace(turn.Content) ||
                turn.Content.Length > MaxMessageLength))
            throw new ArgumentException("Chat history is invalid.", nameof(conversation));
    }
}
