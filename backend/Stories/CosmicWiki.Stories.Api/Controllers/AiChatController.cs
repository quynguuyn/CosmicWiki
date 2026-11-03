using System.ComponentModel.DataAnnotations;
using CosmicWiki.Stories.Application.Features.AiChat;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Stories.Api.Controllers;

[ApiController]
[Route("api/stories/ai-chat")]
public sealed class AiChatController(ISender sender) : ControllerBase
{
    [HttpPost("messages")]
    [EnableRateLimiting("ai-chat")]
    public async Task<IActionResult> SendMessage(
        [FromBody] AiChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Messages.Count is < 1 or > 12 ||
            request.Messages[^1].Role != "user" ||
            request.Messages.Any(turn =>
                turn.Role is not ("user" or "assistant") ||
                string.IsNullOrWhiteSpace(turn.Content) ||
                turn.Content.Length > 4000))
            return BadRequest(new { message = "Chat history is invalid." });

        try
        {
            var reply = await sender.Send(
                new AskCosmicAssistantCommand(request.Messages),
                cancellationToken);
            return Ok(new AiChatResponse(reply));
        }
        catch (AiChatUnavailableException exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    code = exception.IsConfigured ? "temporarily_unavailable" : "not_configured",
                    message = exception.IsConfigured
                        ? "AI chat is temporarily unavailable. Ensure the self-hosted Ollama service is running."
                        : "AI chat is not configured. Set AiChat:OllamaBaseUrl and AiChat:Model."
                });
        }
    }
}

public sealed record AiChatRequest(
    [Required, MinLength(1), MaxLength(12)] IReadOnlyList<AiChatTurn> Messages);

public sealed record AiChatResponse(string Reply);
