using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;
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
        var (title, message) = ContentFor(order.Id, order.Status);

        var notification = OrderNotification.Create(
            order.CustomerId,
            order.Id,
            title,
            message,
            order.Status.ToString());
        db.OrderNotifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        var response = ToResponse(notification);
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

    public static (string Title, string Message) ContentFor(Guid orderId, OrderStatus status) =>
        status switch
        {
            OrderStatus.PendingPayment => ("Đơn hàng đã tạo", $"Đơn hàng #{ShortId(orderId)} đang chờ thanh toán."),
            OrderStatus.Paid => ("Đã nhận thanh toán", $"Đơn hàng #{ShortId(orderId)} đã thanh toán và đang chờ xác nhận."),
            OrderStatus.Confirmed => ("Đơn hàng đã xác nhận", $"Đơn hàng #{ShortId(orderId)} đã được xác nhận và đang chờ nhân viên nhận giao."),
            OrderStatus.Shipping => ("Đơn hàng đang giao", $"Đơn hàng #{ShortId(orderId)} đang được giao đến bạn."),
            OrderStatus.Delivered => ("Đơn hàng đã giao", $"Đơn hàng #{ShortId(orderId)} đã được giao thành công."),
            OrderStatus.Returned => ("Đơn hàng đã trả", $"Đơn hàng #{ShortId(orderId)} đã được chuyển sang trạng thái trả hàng."),
            OrderStatus.Cancelled => ("Đơn hàng đã hủy", $"Đơn hàng #{ShortId(orderId)} đã bị hủy."),
            OrderStatus.PaymentFailed => ("Thanh toán thất bại", $"Thanh toán cho đơn hàng #{ShortId(orderId)} không thành công."),
            _ => throw new InvalidOperationException($"Notifications are not supported for order status '{status}'.")
        };

    public static object ToResponse(OrderNotification notification) => new
    {
        notification.Id,
        notification.OrderId,
        notification.Title,
        notification.Message,
        notification.Status,
        notification.CreatedAtUtc,
        notification.ReadAtUtc,
        notification.IsRead
    };

    private static string ShortId(Guid orderId) => orderId.ToString("N")[..8].ToUpperInvariant();
}
