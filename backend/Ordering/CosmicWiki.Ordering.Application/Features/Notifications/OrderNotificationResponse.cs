using CosmicWiki.Ordering.Domain.Entities;

namespace CosmicWiki.Ordering.Application.Features.Notifications;

public sealed record OrderNotificationResponse(
    Guid Id,
    Guid OrderId,
    string Title,
    string Message,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc,
    bool IsRead)
{
    public static OrderNotificationResponse From(OrderNotification notification) =>
        new(
            notification.Id,
            notification.OrderId,
            notification.Title,
            notification.Message,
            notification.Status,
            notification.CreatedAtUtc,
            notification.ReadAtUtc,
            notification.IsRead);
}

public sealed record OrderNotificationsResponse(
    IReadOnlyCollection<OrderNotificationResponse> Items,
    int UnreadCount);
