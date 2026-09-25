using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Product.Domain.Entities;

public sealed class ProductImage : Entity<Guid>
{
    public ProductImage() : base(Guid.Empty)
    {
    }
    
    private ProductImage(
        Guid id,
        Guid productId,
        string url,
        int displayOrder)
        : base(id)
    {
        ProductId = productId;
        Url = url;
        Urls = new[] { url };
        DisplayOrder = displayOrder;
    }

    public Guid ProductId { get; private set; }

    public string Url { get; private set; }

    public IReadOnlyCollection<string> Urls { get; private set; } =
        Array.Empty<string>();

    public int DisplayOrder { get; private set; }

    public static ProductImage Create(
        Guid productId,
        string url,
        int displayOrder = 0)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));

        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder));

        return new ProductImage(
            Guid.NewGuid(),
            productId,
            url.Trim(),
            displayOrder);
    }

    public void Update(
        string url,
        int displayOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder));

        Url = url.Trim();
        Urls = new[] { Url };
        DisplayOrder = displayOrder;
    }
}