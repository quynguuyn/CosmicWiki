namespace CosmicWiki.Ordering.Domain.Entities;

public sealed class OrderLine
{
    private OrderLine() { }

    private OrderLine(Guid id, Guid productId, string sku, string name, decimal unitPrice, int quantity)
    {
        Id = id;
        ProductId = productId;
        SKU = sku;
        ProductName = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string SKU { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    public static OrderLine Create(Guid productId, string sku, string name, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product ID is required.", nameof(productId));
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (sku.Length > 100) throw new ArgumentException("SKU cannot exceed 100 characters.", nameof(sku));
        if (name.Length > 300) throw new ArgumentException("Product name cannot exceed 300 characters.", nameof(name));
        if (sku.Any(char.IsControl)) throw new ArgumentException("SKU contains invalid characters.", nameof(sku));
        if (name.Any(char.IsControl)) throw new ArgumentException("Product name contains invalid characters.", nameof(name));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > 10000) throw new ArgumentOutOfRangeException(nameof(quantity));
        return new OrderLine(Guid.NewGuid(), productId, sku.Trim(), name.Trim(), unitPrice, quantity);
    }
}
