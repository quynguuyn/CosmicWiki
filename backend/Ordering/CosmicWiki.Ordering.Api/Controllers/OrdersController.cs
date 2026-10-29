using CosmicWiki.Ordering.Application.Features.CreateOrder;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.Ordering.Infrastructure.Processing;
using CosmicWiki.SharedKernel.Authorization;
using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = Permissions.OrderingWrite)]
public sealed class OrdersController(
    ISender sender,
    IOrderRepository orders,
    IOptions<OrderLifecycleOptions> lifecycleOptions) : ControllerBase
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
                request.Lines),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> Get(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null)
            return NotFound();

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
            order.DeliveryLatitude,
            order.DeliveryLongitude,
            order.DeliveryLocationUpdatedAtUtc,
            statusHistory = order.StatusHistory
                .OrderBy(x => x.ChangedAtUtc)
                .ThenBy(x => x.Id)
                .Select(x => new { status = x.Status.ToString(), x.ChangedAtUtc }),
            paymentExpiresAtUtc = order.CreatedAtUtc.AddMinutes(lifecycleOptions.Value.PendingPaymentTimeoutMinutes)
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var authenticatedCustomerId))
            return Forbid();

        var customerOrders = await orders.GetByCustomerIdAsync(authenticatedCustomerId, cancellationToken);

        return Ok(customerOrders.Select(o => new
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
            o.DeliveryLatitude,
            o.DeliveryLongitude,
            o.DeliveryLocationUpdatedAtUtc,
            statusHistory = o.StatusHistory
                .OrderBy(x => x.ChangedAtUtc)
                .ThenBy(x => x.Id)
                .Select(x => new { status = x.Status.ToString(), x.ChangedAtUtc }),
            paymentExpiresAtUtc = o.CreatedAtUtc.AddMinutes(lifecycleOptions.Value.PendingPaymentTimeoutMinutes)
        }));
    }

    [HttpPatch("{orderId:guid}/status")]
    [Authorize(Policy = Permissions.ShippingWrite)]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest? request,
        [FromServices] IOrderEmailSender emailSender,
        CancellationToken cancellationToken)
    {
        if (request is null || !Enum.TryParse<OrderStatus>(request.Status, true, out var nextStatus))
            return BadRequest("A valid order status is required.");
        if (nextStatus == OrderStatus.Shipping)
            return Conflict(new { message = "A delivery person must claim the order from the delivery dashboard." });

        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null)
            return NotFound();

        try
        {
            if (!order.TransitionTo(nextStatus))
                return Ok(new { orderId = order.Id, status = order.Status.ToString() });

            await orders.SaveChangesAsync(cancellationToken);
            await emailSender.SendOrderStatusChangedAsync(
                order.Id,
                order.CustomerEmail,
                order.TotalAmount,
                order.Currency,
                order.Status.ToString(),
                cancellationToken);
            return Ok(new { orderId = order.Id, status = order.Status.ToString() });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string Currency,
    IReadOnlyCollection<CreateOrderLineRequest> Lines);

public sealed record UpdateOrderStatusRequest(string Status);
