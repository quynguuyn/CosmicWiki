using CosmicWiki.Product.Domain.Enums;
using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Product.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductImage> _images = [];

    private Product() : base(Guid.Empty)
    {
    }
    
    private Product(
        Guid id,
        string sku,
        string name,
        string slug,
        string? description,
        decimal originalPrice,
        decimal salePrice,
        int stockQuantity,
        ProductStatus status,
        Guid categoryId)
        : base(id)
    {
        SKU = sku;
        Name = name;
        Slug = slug;
        Description = description;
        OriginalPrice = originalPrice;
        SalePrice = salePrice;
        StockQuantity = stockQuantity;
        Status = status;
        CategoryId = categoryId;
    }

    public string SKU { get; private set; }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public string? Description { get; private set; }

    public decimal OriginalPrice { get; private set; }

    public decimal SalePrice { get; private set; }

    public int StockQuantity { get; private set; }

    public ProductStatus Status { get; private set; }

    public Guid CategoryId { get; private set; }

    public IReadOnlyCollection<ProductImage> Images =>
        _images.AsReadOnly();

    public static Product Create(
        string sku,
        string name,
        string slug,
        decimal originalPrice,
        decimal salePrice,
        int stockQuantity,
        Guid categoryId,
        string? description = null,
        ProductStatus status = ProductStatus.Draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        if (originalPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(originalPrice));

        if (salePrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(salePrice));

        if (salePrice > originalPrice)
            throw new ArgumentException(
                "Sale price cannot be greater than original price.");

        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(stockQuantity));

        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "Category ID cannot be empty.",
                nameof(categoryId));

        return new Product(
            Guid.NewGuid(),
            sku.Trim(),
            name.Trim(),
            slug.Trim(),
            description?.Trim(),
            originalPrice,
            salePrice,
            stockQuantity,
            status,
            categoryId);
    }

    public void Update(
        string sku,
        string name,
        string slug,
        string? description,
        decimal originalPrice,
        decimal salePrice,
        int stockQuantity,
        Guid categoryId,
        ProductStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        if (originalPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(originalPrice));

        if (salePrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(salePrice));

        if (salePrice > originalPrice)
            throw new ArgumentException(
                "Sale price cannot be greater than original price.");

        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(stockQuantity));

        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "Category ID cannot be empty.",
                nameof(categoryId));

        SKU = sku.Trim();
        Name = name.Trim();
        Slug = slug.Trim();
        Description = description?.Trim();
        OriginalPrice = originalPrice;
        SalePrice = salePrice;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        Status = status;
    }

    public void ChangePrice(
        decimal originalPrice,
        decimal salePrice)
    {
        if (originalPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(originalPrice));

        if (salePrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(salePrice));

        if (salePrice > originalPrice)
            throw new ArgumentException(
                "Sale price cannot be greater than original price.");

        OriginalPrice = originalPrice;
        SalePrice = salePrice;
    }

    public void ChangeStock(int stockQuantity)
    {
        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(stockQuantity));

        StockQuantity = stockQuantity;
    }

    public void ChangeStatus(ProductStatus status)
    {
        Status = status;
    }

    public void ChangeCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "Category ID cannot be empty.",
                nameof(categoryId));

        CategoryId = categoryId;
    }

    public void AddImage(ProductImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (image.ProductId != Id)
            throw new ArgumentException(
                "Image does not belong to this product.",
                nameof(image));

        _images.Add(image);
    }
    
    public void UpdateImage(
        Guid imageId,
        string url,
        int displayOrder)
    {
        var image = _images
            .SingleOrDefault(x => x.Id == imageId);

        if (image is null)
        {
            throw new KeyNotFoundException(
                $"Product image '{imageId}' was not found.");
        }

        image.Update(url, displayOrder);
    }

    public void RemoveImage(Guid imageId)
    {
        _images.RemoveAll(x => x.Id == imageId);
    }
}