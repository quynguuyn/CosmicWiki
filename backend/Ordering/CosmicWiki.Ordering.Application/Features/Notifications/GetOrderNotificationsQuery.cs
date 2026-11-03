using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Notifications;

public sealed record GetOrderNotificationsQuery(Guid CustomerId) : IRequest<OrderNotificationsResponse>;

public sealed class GetOrderNotificationsQueryHandler(
    IOrderNotificationRepository notifications) : IRequestHandler<GetOrderNotificationsQuery, OrderNotificationsResponse>
{
    private const int MaxNotificationCount = 30;

    public async Task<OrderNotificationsResponse> Handle(
        GetOrderNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        await BackfillFromOrderHistoryAsync(request.CustomerId, cancellationToken);

        var items = await notifications.GetByCustomerIdAsync(
            request.CustomerId,
            MaxNotificationCount,
            cancellationToken);
        var unreadCount = await notifications.CountUnreadByCustomerIdAsync(
            request.CustomerId,
            cancellationToken);

        return new(
            items.Select(OrderNotificationResponse.From).ToArray(),
            unreadCount);
    }

    private async Task BackfillFromOrderHistoryAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var recentOrderIds = await notifications.GetRecentOrderIdsAsync(
            customerId,
            MaxNotificationCount,
            cancellationToken);
        var changes = await notifications.GetStatusHistoryAsync(recentOrderIds, cancellationToken);
        var existingStatuses = await notifications.GetExistingOrderStatusesAsync(customerId, cancellationToken);
        var existingKeys = existingStatuses.ToHashSet();
        var hasBackfill = false;

        foreach (var change in changes)
        {
            var status = change.Status.ToString();
            if (!existingKeys.Add((change.OrderId, status)))
                continue;

            var (title, message) = OrderNotificationContent.For(change.OrderId, change.Status);
            notifications.Add(OrderNotification.Create(
                customerId,
                change.OrderId,
                title,
                message,
                status,
                change.ChangedAtUtc));
            hasBackfill = true;
        }

        if (hasBackfill)
            await notifications.SaveChangesAsync(cancellationToken);
    }
}
