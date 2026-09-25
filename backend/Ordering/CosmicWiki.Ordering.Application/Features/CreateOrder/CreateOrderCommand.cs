using MediatR;

namespace CosmicWiki.Ordering.Application.Features.CreateOrder;

public sealed record CreateOrderLineRequest(Guid ProductId, int Quantity);
public sealed record CreateOrderCommand(Guid CustomerId, string Currency, string IdempotencyKey, IReadOnlyCollection<CreateOrderLineRequest> Lines) : IRequest<CreateOrderResult>;
public sealed record CreateOrderResult(Guid OrderId, string PaymentIntentId, decimal TotalAmount, string Currency, string Status);
