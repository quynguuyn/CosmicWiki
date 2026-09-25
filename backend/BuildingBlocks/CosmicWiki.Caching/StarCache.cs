using System.Threading;
using System.Threading.Tasks;

namespace CosmicWiki.Caching;

public interface IStarCache
{
    Task<string> GetListKeyAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task InvalidateListAsync(
        CancellationToken cancellationToken = default);

    Task InvalidateAsync(
        Guid starId,
        CancellationToken cancellationToken = default);
}

internal sealed class StarCache(ICacheStore store) : IStarCache
{
    private const string ListVersionKey = "star:list:version";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public async Task<string> GetListKeyAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var version = await store.GetAsync<string>(
            ListVersionKey,
            cancellationToken);

        if (version is null)
        {
            version = Guid.NewGuid().ToString("N");
            await store.SetAsync(
                ListVersionKey,
                version,
                Lifetime,
                cancellationToken);
        }

        return $"star:list:{version}:{page}:{pageSize}";
    }

    public async Task InvalidateListAsync(
        CancellationToken cancellationToken = default)
    {
        await store.SetAsync(
            ListVersionKey,
            Guid.NewGuid().ToString("N"),
            Lifetime,
            cancellationToken);
    }

    public Task InvalidateAsync(
        Guid starId,
        CancellationToken cancellationToken = default)
    {
        return InvalidateCoreAsync(starId, cancellationToken);
    }

    private async Task InvalidateCoreAsync(
        Guid starId,
        CancellationToken cancellationToken)
    {
        await store.RemoveAsync(
            $"star:{starId:N}",
            cancellationToken);
            
        await InvalidateListAsync(cancellationToken);
    }
}
