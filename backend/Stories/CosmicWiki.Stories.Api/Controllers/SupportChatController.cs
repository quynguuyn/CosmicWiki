using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Stories.Application.Features.SupportChat;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Stories.Api.Controllers;

[ApiController]
[Route("api/stories/support-chat")]
public sealed class SupportChatController(ISender sender) : ControllerBase
{
    [HttpPost("conversations")]
    [AllowAnonymous]
    public async Task<IActionResult> OpenConversation(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (User.Identity?.IsAuthenticated == true && userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new OpenSupportConversationCommand(
                userId,
                GetDisplayName("Khách vãng lai"),
                Request.Headers["X-Guest-Chat-Token"].FirstOrDefault()),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConversation(
        Guid conversationId,
        [FromQuery] DateTime? afterUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCustomerSupportConversationQuery(
                conversationId,
                GetUserId(),
                Request.Headers["X-Guest-Chat-Token"].FirstOrDefault(),
                afterUtc),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    [AllowAnonymous]
    public async Task<IActionResult> SendCustomerMessage(
        Guid conversationId,
        [FromBody] SendSupportChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest(new { message = "Message cannot be empty." });
        var result = await sender.Send(
            new SendCustomerSupportMessageCommand(
                conversationId,
                GetUserId(),
                Request.Headers["X-Guest-Chat-Token"].FirstOrDefault(),
                GetDisplayName("Khách vãng lai"),
                request.Body),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/close")]
    [AllowAnonymous]
    public async Task<IActionResult> CloseCustomerConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var closed = await sender.Send(
            new CloseCustomerSupportConversationCommand(
                conversationId,
                GetUserId(),
                Request.Headers["X-Guest-Chat-Token"].FirstOrDefault()),
            cancellationToken);
        return closed ? NoContent() : NotFound();
    }

    [HttpGet("agent/conversations")]
    [Authorize(Policy = Permissions.SupportChatManage)]
    public async Task<IActionResult> GetAgentConversations(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await sender.Send(
                new GetAgentSupportConversationsQuery(userId.Value),
                cancellationToken));
    }

    [HttpPost("agent/presence")]
    [Authorize(Policy = Permissions.SupportChatManage)]
    public async Task<IActionResult> SetAgentPresence(
        [FromBody] SupportAgentPresenceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        await sender.Send(
            new SetSupportAgentPresenceCommand(
                userId.Value,
                GetDisplayName("Nhân viên hỗ trợ"),
                request.AcceptingChats),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("agent/conversations/{conversationId:guid}/claim")]
    [Authorize(Policy = Permissions.SupportChatManage)]
    public async Task<IActionResult> ClaimConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var claimed = await sender.Send(
            new ClaimSupportConversationCommand(
                conversationId,
                userId.Value,
                GetDisplayName("Nhân viên hỗ trợ")),
            cancellationToken);
        return claimed ? NoContent() : Conflict(new { message = "Conversation is no longer available." });
    }

    [HttpGet("agent/conversations/{conversationId:guid}")]
    [Authorize(Policy = Permissions.SupportChatManage)]
    public async Task<IActionResult> GetAgentConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var result = await sender.Send(
            new GetAgentSupportConversationQuery(conversationId, userId.Value),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("agent/conversations/{conversationId:guid}/messages")]
    [Authorize(Policy = Permissions.SupportChatManage)]
    public async Task<IActionResult> SendAgentMessage(
        Guid conversationId,
        [FromBody] SendSupportChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest(new { message = "Message cannot be empty." });
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var result = await sender.Send(
            new SendAgentSupportMessageCommand(
                conversationId,
                userId.Value,
                GetDisplayName("Nhân viên hỗ trợ"),
                request.Body),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("agent/conversations/{conversationId:guid}/close")]
    [Authorize(Policy = Permissions.SupportChatManage)]
    public async Task<IActionResult> CloseAgentConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var closed = await sender.Send(
            new CloseAgentSupportConversationCommand(conversationId, userId.Value),
            cancellationToken);
        return closed ? NoContent() : NotFound();
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue("sub"), out var userId) && userId != Guid.Empty
            ? userId
            : null;

    private string GetDisplayName(string fallback)
    {
        var displayName = User.FindFirstValue("unique_name");
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = fallback;
        return displayName.Length <= 256 ? displayName : displayName[..256];
    }
}

public sealed record SendSupportChatMessageRequest(
    [Required, StringLength(4000)] string Body);

public sealed record SupportAgentPresenceRequest(bool AcceptingChats);
