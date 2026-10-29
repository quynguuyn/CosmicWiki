using System.IdentityModel.Tokens.Jwt;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;
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

        await BackfillFromOrderHistoryAsync(customerId, cancellationToken);

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

    private async Task BackfillFromOrderHistoryAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var recentOrderIds = await db.Orders
            .AsNoTracking()
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => order.Id)
            .Take(30)
            .ToListAsync(cancellationToken);
        var changes = await db.OrderStatusHistory
            .AsNoTracking()
            .Where(history => recentOrderIds.Contains(history.OrderId))
            .Select(history => new { history.OrderId, history.Status, history.ChangedAtUtc })
            .ToListAsync(cancellationToken);

        var existing = await db.OrderNotifications
            .Where(item => item.CustomerId == customerId)
            .Select(item => new { item.OrderId, item.Status })
            .ToListAsync(cancellationToken);
        var existingKeys = existing
            .Select(item => (item.OrderId, item.Status))
            .ToHashSet();

        foreach (var change in changes)
        {
            var status = change.Status.ToString();
            if (!existingKeys.Add((change.OrderId, status)))
                continue;

            var (title, message) = OrderNotificationPublisher.ContentFor(change.OrderId, change.Status);
            db.OrderNotifications.Add(OrderNotification.Create(
                customerId,
                change.OrderId,
                title,
                message,
                status,
                change.ChangedAtUtc));
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(cancellationToken);
    }
}
