using CosmicWiki.Ordering.Application.Abstractions;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Orders;

public sealed record ValidatePaymentIntentQuery(
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency) : IRequest<bool>;

public sealed class ValidatePaymentIntentQueryHandler(IOrderPaymentIntentValidator validator)
    : IRequestHandler<ValidatePaymentIntentQuery, bool>
{
    public Task<bool> Handle(
        ValidatePaymentIntentQuery request,
        CancellationToken cancellationToken) =>
        validator.IsPaymentIntentValidAsync(
            request.OrderId,
            request.PaymentIntentId,
            request.Amount,
            request.Currency,
            cancellationToken);
}
