using CosmicWiki.Ordering.Application.Abstractions;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Notifications;

public sealed record MarkAllOrderNotificationsReadCommand(Guid CustomerId) : IRequest;

public sealed class MarkAllOrderNotificationsReadCommandHandler(
    IOrderNotificationRepository notifications)
    : IRequestHandler<MarkAllOrderNotificationsReadCommand>
{
    public async Task Handle(
        MarkAllOrderNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        var unread = await notifications.GetUnreadByCustomerIdAsync(
            request.CustomerId,
            cancellationToken);
        foreach (var notification in unread)
            notification.MarkRead();

        if (unread.Count > 0)
            await notifications.SaveChangesAsync(cancellationToken);
    }
}
