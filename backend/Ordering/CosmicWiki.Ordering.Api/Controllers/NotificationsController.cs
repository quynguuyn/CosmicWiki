using System.IdentityModel.Tokens.Jwt;
using CosmicWiki.Ordering.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Forbid();

        var result = await sender.Send(new GetOrderNotificationsQuery(customerId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Forbid();

        var notification = await sender.Send(
            new MarkOrderNotificationReadCommand(customerId, notificationId),
            cancellationToken);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Forbid();

        await sender.Send(new MarkAllOrderNotificationsReadCommand(customerId), cancellationToken);
        return NoContent();
    }

    private bool TryGetCustomerId(out Guid customerId) =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out customerId)
        && customerId != Guid.Empty;
}
