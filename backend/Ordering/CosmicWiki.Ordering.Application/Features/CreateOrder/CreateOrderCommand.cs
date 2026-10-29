using MediatR;

namespace CosmicWiki.Ordering.Application.Features.CreateOrder;

public sealed record CreateOrderLineRequest(Guid ProductId, int Quantity);
public sealed record CreateOrderCommand(
    Guid CustomerId,
    string CustomerEmail,
    string Currency,
    string IdempotencyKey,
    IReadOnlyCollection<CreateOrderLineRequest> Lines,
    string? DeliveryAddress = null) : IRequest<CreateOrderResult>;
public sealed record CreateOrderResult(
    Guid OrderId,
    string PaymentIntentId,
    decimal TotalAmount,
    string Currency,
    string Status,
    string? ClientSecret = null);
