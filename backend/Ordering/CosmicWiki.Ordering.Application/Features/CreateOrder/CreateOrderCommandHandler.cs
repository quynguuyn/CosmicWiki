using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Application.Events;
using CosmicWiki.Messaging.Abstractions;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CosmicWiki.Ordering.Application.Features.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orders;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IProductSnapshotProvider _products;
    private readonly IInventoryReservationService _inventory;
    private readonly IOutboxWriter _outbox;

    public CreateOrderCommandHandler(
        IOrderRepository orders,
        IPaymentProvider paymentProvider,
        IProductSnapshotProvider products,
        IInventoryReservationService inventory,
        IOutboxWriter outbox)
    {
        _orders = orders;
        _paymentProvider = paymentProvider;
        _products = products;
        _inventory = inventory;
        _outbox = outbox;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var requestFingerprint = ComputeFingerprint(request);

        var existing = await _orders.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.RequestFingerprint, requestFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The Idempotency-Key was already used for a different order request.");

            return new(existing.Id, existing.PaymentIntentId ?? string.Empty, existing.TotalAmount, existing.Currency, existing.Status.ToString());
        }

        var snapshots = new List<(CreateOrderLineRequest Request, ProductSnapshot Product)>();
        foreach (var line in request.Lines)
        {
            var product = await _products.GetAsync(line.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product '{line.ProductId}' was not found.");

            if (!product.IsActive)
                throw new InvalidOperationException($"Product '{line.ProductId}' is not available.");

            if (line.Quantity > product.AvailableStock)
                throw new InvalidOperationException($"Product '{line.ProductId}' does not have enough stock.");

            snapshots.Add((line, product));
        }

        var lines = snapshots.Select(x => OrderLine.Create(
            x.Product.ProductId,
            x.Product.SKU,
            x.Product.Name,
            x.Product.UnitPrice,
            x.Request.Quantity));
        var order = Order.Create(request.CustomerId, request.Currency, request.IdempotencyKey, requestFingerprint, lines);
        var reserved = await _inventory.ReserveAsync(
            order.Id,
            order.Lines.Select(x => new InventoryReservationLine(x.ProductId, x.Quantity)).ToArray(),
            cancellationToken);
        if (!reserved)
            throw new InvalidOperationException("One or more products no longer have enough stock.");

        try
        {
            var intent = await _paymentProvider.CreatePaymentIntentAsync(
                order.Id, order.TotalAmount, order.Currency, request.IdempotencyKey, cancellationToken);
            order.AttachPaymentIntent(intent.PaymentIntentId);
            _orders.Add(order);
            _outbox.Add(new OrderCreatedIntegrationEvent(
                Guid.NewGuid(), 1, DateTime.UtcNow, order.Id, order.CustomerId, order.TotalAmount, order.Currency), "cosmicwiki.ordering");
            await _orders.SaveChangesAsync(cancellationToken);
            return new(order.Id, intent.PaymentIntentId, order.TotalAmount, order.Currency, order.Status.ToString());
        }
        catch
        {
            await _inventory.ReleaseAsync(order.Id, CancellationToken.None);
            throw;
        }
    }

    private static void ValidateRequest(CreateOrderCommand request)
    {
        if (request.Lines is null)
            throw new ArgumentException("Order lines are required.", nameof(request));
        if (request.Lines.Count is < 1 or > 100)
            throw new ArgumentException("Order must contain between 1 and 100 lines.", nameof(request));
        if (request.Lines.Any(x =>
            x.Quantity is < 1 or > 10000 ||
            x.ProductId == Guid.Empty))
            throw new ArgumentException("Order contains an invalid line.", nameof(request));
        if (request.Lines.Select(x => x.ProductId).Distinct().Count() != request.Lines.Count)
            throw new ArgumentException("An order cannot contain duplicate products.", nameof(request));

        if (request.Currency is null ||
            request.Currency.Length != 3 ||
            request.Currency.Any(c => !char.IsAsciiLetter(c)))
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(request));

        if (request.IdempotencyKey is null ||
            request.IdempotencyKey.Length is < 1 or > 200 ||
            request.IdempotencyKey != request.IdempotencyKey.Trim() ||
            request.IdempotencyKey.Any(char.IsControl))
            throw new ArgumentException("A valid Idempotency-Key header is required.", nameof(request));
    }

    private static string ComputeFingerprint(CreateOrderCommand request)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            request.CustomerId,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Lines = request.Lines.Select(x => new
            {
                x.ProductId,
                x.Quantity
            })
        });

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
