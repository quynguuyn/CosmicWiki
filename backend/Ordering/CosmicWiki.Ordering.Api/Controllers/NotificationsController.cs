using System.IdentityModel.Tokens.Jwt;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(OrderingDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Forbid();

        var notifications = await db.OrderNotifications
            .AsNoTracking()
            .Where(item => item.CustomerId == customerId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(30)
            .ToListAsync(cancellationToken);
        var unreadCount = await db.OrderNotifications
            .CountAsync(item => item.CustomerId == customerId && item.ReadAtUtc == null, cancellationToken);

        return Ok(new
        {
            items = notifications.Select(OrderNotificationPublisher.ToResponse),
            unreadCount
        });
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Forbid();

        var notification = await db.OrderNotifications
            .SingleOrDefaultAsync(
                item => item.Id == notificationId && item.CustomerId == customerId,
                cancellationToken);
        if (notification is null)
            return NotFound();

        notification.MarkRead();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(OrderNotificationPublisher.ToResponse(notification));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Forbid();

        var unread = await db.OrderNotifications
            .Where(item => item.CustomerId == customerId && item.ReadAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var notification in unread)
            notification.MarkRead();

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool TryGetCustomerId(out Guid customerId) =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out customerId)
        && customerId != Guid.Empty;
}
