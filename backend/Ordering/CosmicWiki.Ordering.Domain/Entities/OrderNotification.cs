namespace CosmicWiki.Ordering.Domain.Entities;

public sealed class OrderNotification
{
    private OrderNotification() { }

    private OrderNotification(
        Guid id,
        Guid customerId,
        Guid orderId,
        string title,
        string message,
        string status,
        DateTime createdAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        OrderId = orderId;
        Title = title;
        Message = message;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public bool IsRead => ReadAtUtc.HasValue;

    public static OrderNotification Create(
        Guid customerId,
        Guid orderId,
        string title,
        string message,
        string status,
        DateTime? createdAtUtc = null)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (orderId == Guid.Empty) throw new ArgumentException("Order ID is required.", nameof(orderId));
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        return new OrderNotification(
            Guid.NewGuid(),
            customerId,
            orderId,
            title.Trim(),
            message.Trim(),
            status.Trim(),
            createdAtUtc ?? DateTime.UtcNow);
    }

    public void MarkRead()
    {
        ReadAtUtc ??= DateTime.UtcNow;
    }
}
