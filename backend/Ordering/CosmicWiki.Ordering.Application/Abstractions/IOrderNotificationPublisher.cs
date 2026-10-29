using CosmicWiki.Ordering.Domain.Entities;

namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IOrderNotificationPublisher
{
    Task PublishStatusChangedAsync(Order order, CancellationToken cancellationToken);
}
