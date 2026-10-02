using System.Threading;
using System.Threading.Tasks;

namespace CosmicWiki.Ordering.Application.Abstractions;

public interface ICartRepository
{
    Task<string?> GetCartAsync(string customerId, CancellationToken cancellationToken = default);
    Task SaveCartAsync(string customerId, string cartJson, CancellationToken cancellationToken = default);
    Task DeleteCartAsync(string customerId, CancellationToken cancellationToken = default);
}
