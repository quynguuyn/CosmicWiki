namespace CosmicWiki.Product.Infrastructure.Persistence;

public sealed class InventoryReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ReleasedAtUtc { get; set; }
}
