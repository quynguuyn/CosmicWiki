using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Application.Features.Notifications;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Ordering.Infrastructure.Notifications;

public sealed class OrderNotificationPublisher(
    OrderingDbContext db,
    IHubContext<OrderNotificationsHub> hub,
    ILogger<OrderNotificationPublisher> logger) : IOrderNotificationPublisher
{
    public async Task PublishStatusChangedAsync(Order order, CancellationToken cancellationToken)
    {
        var (title, message) = OrderNotificationContent.For(order.Id, order.Status);

        var notification = OrderNotification.Create(
            order.CustomerId,
            order.Id,
            title,
            message,
            order.Status.ToString());
        db.OrderNotifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        var response = OrderNotificationResponse.From(notification);
        try
        {
            await hub.Clients.Group($"user:{order.CustomerId:D}")
                .SendAsync("NotificationReceived", response, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception,
                "Could not send real-time notification {NotificationId} for order {OrderId}.",
                notification.Id,
                order.Id);
        }
    }

}
