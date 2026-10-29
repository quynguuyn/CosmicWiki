using CosmicWiki.Ordering.Application.Events;

namespace CosmicWiki.Ordering.Infrastructure.Notifications;

public interface IOrderEmailSender
{
    Task SendOrderCreatedAsync(
        OrderCreatedIntegrationEvent notification,
        CancellationToken cancellationToken);

    Task SendOrderStatusChangedAsync(
        Guid orderId,
        string? customerEmail,
        decimal totalAmount,
        string currency,
        string status,
        IReadOnlyCollection<OrderCreatedLine> lines,
        CancellationToken cancellationToken);
}
