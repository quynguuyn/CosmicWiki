using System.IdentityModel.Tokens.Jwt;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Application.Features.Orders;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/delivery/orders")]
[Authorize(Policy = Permissions.DeliveryWrite)]
public sealed class DeliveryController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        if (!TryGetDeliveryPersonId(out var deliveryPersonId))
            return Forbid();

        var result = await sender.Send(new GetDeliveryOrdersQuery(deliveryPersonId), cancellationToken);
        return Ok(result.Available.Select(order => ToResponse(order, true))
            .Concat(result.Assigned.Select(order => ToResponse(order, false))));
    }

    [HttpPost("{orderId:guid}/claim")]
    public async Task<IActionResult> Claim(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetDeliveryPersonId(out var deliveryPersonId))
            return Forbid();

        try
        {
            var deliveryPersonName = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
            var order = await sender.Send(
                new ClaimDeliveryOrderCommand(orderId, deliveryPersonId, deliveryPersonName),
                cancellationToken);
            if (order is null)
                return NotFound();
            return Ok(ToResponse(order, false));
        }
        catch (DeliveryClaimForbiddenException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = "This order is no longer available for delivery." });
        }
        catch (OrderConcurrencyException)
        {
            return Conflict(new { message = "This order was claimed by another delivery person." });
        }
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

        try
        {
            var order = await sender.Send(
                new UpdateDeliveryLocationCommand(
                    orderId,
                    deliveryPersonId,
                    request.Latitude,
                    request.Longitude),
                cancellationToken);
            if (order is null)
                return NotFound();
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

        try
        {
            var order = await sender.Send(
                new MarkOrderDeliveredCommand(orderId, deliveryPersonId),
                cancellationToken);
            if (order is null)
                return NotFound();
            return Ok(ToResponse(order, false));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (OrderConcurrencyException)
        {
            return Conflict(new { message = "The order status changed. Refresh and try again." });
        }
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
