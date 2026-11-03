using CosmicWiki.Ordering.Application.Abstractions;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Notifications;

public sealed record MarkOrderNotificationReadCommand(Guid CustomerId, Guid NotificationId)
    : IRequest<OrderNotificationResponse?>;

public sealed class MarkOrderNotificationReadCommandHandler(
    IOrderNotificationRepository notifications)
    : IRequestHandler<MarkOrderNotificationReadCommand, OrderNotificationResponse?>
{
    public async Task<OrderNotificationResponse?> Handle(
        MarkOrderNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.GetAsync(
            request.NotificationId,
            request.CustomerId,
            cancellationToken);
        if (notification is null)
            return null;

        notification.MarkRead();
        await notifications.SaveChangesAsync(cancellationToken);
        return OrderNotificationResponse.From(notification);
    }
}
