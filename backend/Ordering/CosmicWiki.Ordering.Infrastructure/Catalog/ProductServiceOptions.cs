namespace CosmicWiki.Ordering.Infrastructure.Catalog;

public sealed class ProductServiceOptions
{
    public const string SectionName = "ProductService";

    public string BaseUrl { get; set; } = "http://localhost:5080";

    public string ServiceKey { get; set; } = string.Empty;
}
