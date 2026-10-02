using System;
using System.Threading;
using System.Threading.Tasks;
using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IDistributedCache _cache;
    private const string CartPrefix = "cart:";

    public CartRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetCartAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync($"{CartPrefix}{customerId}", cancellationToken);
    }

    public async Task SaveCartAsync(string customerId, string cartJson, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        await _cache.SetStringAsync($"{CartPrefix}{customerId}", cartJson, options, cancellationToken);
    }

    public async Task DeleteCartAsync(string customerId, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"{CartPrefix}{customerId}", cancellationToken);
    }
}
