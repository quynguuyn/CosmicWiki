using CosmicWiki.Ordering.Application.Features.CreateOrder;
using CosmicWiki.Ordering.Application.Events;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Application.Features.Orders;
using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.SharedKernel.Authorization;
using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = Permissions.OrderingWrite)]
public sealed class OrdersController(
    ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateOrderResult>> Create(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] CreateOrderRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Order payload is required.");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest("Idempotency-Key header is required.");

        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (!Guid.TryParse(subject, out var authenticatedCustomerId) ||
            authenticatedCustomerId != request.CustomerId ||
            string.IsNullOrWhiteSpace(email))
            return Forbid();

        var result = await sender.Send(
            new CreateOrderCommand(
                request.CustomerId,
                email,
                request.Currency,
                idempotencyKey,
                request.Lines,
                request.DeliveryAddress),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> Get(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderQuery(orderId), cancellationToken);
        if (result is null)
            return NotFound();
        var order = result.Order;

        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var authenticatedCustomerId) ||
            authenticatedCustomerId != order.CustomerId)
            return Forbid();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status.ToString(),
            order.PaymentIntentId,
            order.ClientSecret,
            order.TotalAmount,
            order.Currency,
            order.PaidAtUtc,
            order.DeliveryPersonName,
            order.DeliveryAddress,
            order.DeliveryLatitude,
            order.DeliveryLongitude,
            order.DeliveryLocationUpdatedAtUtc,
            lines = order.Lines.Select(ToLineResponse),
            statusHistory = order.StatusHistory
                .OrderBy(x => x.ChangedAtUtc)
                .ThenBy(x => x.Id)
                .Select(x => new { status = x.Status.ToString(), x.ChangedAtUtc }),
            paymentExpiresAtUtc = result.PaymentExpiresAtUtc
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var authenticatedCustomerId))
            return Forbid();

        var customerOrders = await sender.Send(
            new GetCustomerOrdersQuery(authenticatedCustomerId),
            cancellationToken);

        return Ok(customerOrders.Select(result =>
        {
            var o = result.Order;
            return new
            {
                orderId = o.Id,
                status = o.Status.ToString(),
                o.PaymentIntentId,
                o.ClientSecret,
                o.TotalAmount,
                o.Currency,
                o.PaidAtUtc,
                o.CreatedAtUtc,
                o.DeliveryPersonName,
                o.DeliveryAddress,
                o.DeliveryLatitude,
                o.DeliveryLongitude,
                o.DeliveryLocationUpdatedAtUtc,
                lines = o.Lines.Select(ToLineResponse),
                statusHistory = o.StatusHistory
                .OrderBy(x => x.ChangedAtUtc)
                .ThenBy(x => x.Id)
                .Select(x => new { status = x.Status.ToString(), x.ChangedAtUtc }),
                paymentExpiresAtUtc = result.PaymentExpiresAtUtc
            };
        }));
    }

    private static object ToLineResponse(Domain.Entities.OrderLine line) => new
    {
        line.ProductId,
        line.SKU,
        line.ProductName,
        line.UnitPrice,
        line.ProductImageUrl,
        line.Quantity,
        lineTotal = line.LineTotal
    };

    [HttpPatch("{orderId:guid}/status")]
    [Authorize(Policy = Permissions.ShippingWrite)]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || !Enum.TryParse<OrderStatus>(request.Status, true, out var nextStatus))
            return BadRequest("A valid order status is required.");
        if (nextStatus == OrderStatus.Shipping)
            return Conflict(new { message = "A delivery person must claim the order from the delivery dashboard." });

        try
        {
            var order = await sender.Send(
                new UpdateOrderStatusCommand(orderId, nextStatus),
                cancellationToken);
            if (order is null)
                return NotFound();
            return Ok(new { orderId = order.Id, status = order.Status.ToString() });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (OrderConcurrencyException)
        {
            return Conflict(new { message = "The order status changed. Refresh and try again." });
        }
    }
}

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string Currency,
    IReadOnlyCollection<CreateOrderLineRequest> Lines,
    string? DeliveryAddress = null);

public sealed record UpdateOrderStatusRequest(string Status);
