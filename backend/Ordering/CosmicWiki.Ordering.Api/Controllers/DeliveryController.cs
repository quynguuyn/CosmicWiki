using System.IdentityModel.Tokens.Jwt;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Application.Events;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/delivery/orders")]
[Authorize(Policy = Permissions.DeliveryWrite)]
public sealed class DeliveryController(
    IOrderRepository orders,
    IOrderEmailSender emailSender,
    IOrderNotificationPublisher notificationPublisher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        if (!TryGetDeliveryPersonId(out var deliveryPersonId))
            return Forbid();

        var available = await orders.GetAvailableForDeliveryAsync(cancellationToken);
        var assigned = await orders.GetByDeliveryPersonIdAsync(deliveryPersonId, cancellationToken);
        return Ok(available.Select(order => ToResponse(order, true))
            .Concat(assigned.Select(order => ToResponse(order, false))));
    }

    [HttpPost("{orderId:guid}/claim")]
    public async Task<IActionResult> Claim(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetDeliveryPersonId(out var deliveryPersonId))
            return Forbid();

        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null)
            return NotFound();

        try
        {
            var deliveryPersonName = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
            if (string.IsNullOrWhiteSpace(deliveryPersonName))
                return Forbid();

            order.AssignDeliveryPerson(deliveryPersonId, deliveryPersonName);
            await orders.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = "This order is no longer available for delivery." });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "This order was claimed by another delivery person." });
        }

        await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
        await emailSender.SendOrderStatusChangedAsync(
            order.Id,
            order.CustomerEmail,
            order.TotalAmount,
            order.Currency,
            order.Status.ToString(),
            ToEmailLines(order),
            cancellationToken);
        return Ok(ToResponse(order, false));
    }

    [HttpPut("{orderId:guid}/location")]
    public async Task<IActionResult> UpdateLocation(
        Guid orderId,
        [FromBody] UpdateDeliveryLocationRequest? request,
        CancellationToken cancellationToken)
    {
        if (!TryGetDeliveryPersonId(out var deliveryPersonId))
            return Forbid();
        if (request is null)
            return BadRequest();

        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null)
            return NotFound();

        try
        {
            order.UpdateDeliveryLocation(deliveryPersonId, request.Latitude, request.Longitude);
            await orders.SaveChangesAsync(cancellationToken);
            return Ok(ToResponse(order, false));
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("{orderId:guid}/delivered")]
    public async Task<IActionResult> MarkDelivered(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetDeliveryPersonId(out var deliveryPersonId))
            return Forbid();

        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null)
            return NotFound();

        try
        {
            order.MarkDeliveredBy(deliveryPersonId);
            await orders.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "The order status changed. Refresh and try again." });
        }

        await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
        await emailSender.SendOrderStatusChangedAsync(
            order.Id,
            order.CustomerEmail,
            order.TotalAmount,
            order.Currency,
            order.Status.ToString(),
            ToEmailLines(order),
            cancellationToken);
        return Ok(ToResponse(order, false));
    }

    private bool TryGetDeliveryPersonId(out Guid deliveryPersonId) =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out deliveryPersonId)
        && deliveryPersonId != Guid.Empty;

    private static DeliveryOrderResponse ToResponse(Order order, bool available) =>
        new(
            order.Id,
            order.Status.ToString(),
            available,
            order.TotalAmount,
            order.Currency,
            order.CreatedAtUtc,
            order.DeliveryPersonName,
            order.DeliveryAddress,
            order.DeliveryLatitude,
            order.DeliveryLongitude,
            order.DeliveryLocationUpdatedAtUtc,
            order.Lines.Select(line => new DeliveryOrderLineResponse(
                line.ProductId,
                line.SKU,
                line.ProductName,
                line.UnitPrice,
                line.ProductImageUrl,
                line.Quantity,
                line.LineTotal)).ToArray());

    private static IReadOnlyCollection<OrderCreatedLine> ToEmailLines(Order order) =>
        order.Lines.Select(line => new OrderCreatedLine(
            line.SKU,
            line.ProductName,
            line.UnitPrice,
            line.Quantity,
            line.LineTotal)).ToArray();
}

public sealed record UpdateDeliveryLocationRequest(double Latitude, double Longitude);

public sealed record DeliveryOrderResponse(
    Guid OrderId,
    string Status,
    bool AvailableToClaim,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAtUtc,
    string? DeliveryPersonName,
    string? DeliveryAddress,
    double? DeliveryLatitude,
    double? DeliveryLongitude,
    DateTime? DeliveryLocationUpdatedAtUtc,
    IReadOnlyCollection<DeliveryOrderLineResponse> Lines);

public sealed record DeliveryOrderLineResponse(
    Guid ProductId,
    string SKU,
    string ProductName,
    decimal UnitPrice,
    string? ProductImageUrl,
    int Quantity,
    decimal LineTotal);
