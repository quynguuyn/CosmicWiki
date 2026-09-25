using CosmicWiki.Ordering.Domain.Entities;

namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken);
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken);
    void Add(Order order);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
