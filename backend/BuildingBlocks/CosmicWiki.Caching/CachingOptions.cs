namespace CosmicWiki.Caching;

public sealed class CachingOptions
{
    public const string SectionName = "Caching";

    public bool Enabled { get; init; }

    /// <summary>TTL cho cache GET /products/:id (giây). Mặc định 30s.</summary>
    public int ProductDetailTtlSeconds { get; init; } = 30;

    /// <summary>TTL cho cache GET /products (giây). Mặc định 30s.</summary>
    public int ProductListTtlSeconds { get; init; } = 30;

    public int StoryDetailTtlSeconds { get; init; } = 60;

    public int StoryListTtlSeconds { get; init; } = 30;

    /// <summary>TTL cho cache chi tiết Catalog (giây). Mặc định 120s.</summary>
    public int CatalogDetailTtlSeconds { get; init; } = 120;

    /// <summary>TTL cho cache danh sách Catalog (giây). Mặc định 60s.</summary>
    public int CatalogListTtlSeconds { get; init; } = 60;

    public RedisOptions Redis { get; init; } = new();
}

public sealed class RedisOptions
{
    public string? ConnectionString { get; init; }
    public string InstanceName { get; init; } = "cosmicwiki:";
}
