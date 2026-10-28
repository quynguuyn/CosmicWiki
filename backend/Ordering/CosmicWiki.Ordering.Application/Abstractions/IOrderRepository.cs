using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;

namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken);
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken);
    void Add(Order order);
    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetAvailableForDeliveryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetByDeliveryPersonIdAsync(Guid deliveryPersonId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetCreatedSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Order> Items, int TotalCount)> GetCreatedSincePageAsync(
        DateTime sinceUtc,
        OrderStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
