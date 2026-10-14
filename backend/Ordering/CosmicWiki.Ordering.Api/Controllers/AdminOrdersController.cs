using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = Permissions.ShippingWrite)]
public sealed class AdminOrdersController(
    IOrderRepository orders,
    IInventoryReservationService inventory,
    IOrderEmailSender emailSender) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var startOfDayUtc = DateTime.UtcNow.Date;
        var today = await orders.GetCreatedSinceAsync(startOfDayUtc, cancellationToken);
        return Ok(today.Select(ToResponse));
    }

    [HttpPatch("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || !Enum.TryParse<OrderStatus>(request.Status, true, out var nextStatus))
            return BadRequest("A valid order status is required.");

        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null)
            return NotFound();

        try
        {
            if (!order.TransitionTo(nextStatus))
                return Ok(ToResponse(order));

            if (nextStatus == OrderStatus.Cancelled)
                await inventory.ReleaseAsync(order.Id, cancellationToken);
            await orders.SaveChangesAsync(cancellationToken);
            await emailSender.SendOrderStatusChangedAsync(
                order.Id, order.CustomerEmail, order.TotalAmount, order.Currency,
                order.Status.ToString(), cancellationToken);
            return Ok(ToResponse(order));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private static object ToResponse(Domain.Entities.Order order) => new
    {
        orderId = order.Id,
        customerId = order.CustomerId,
        customerEmail = order.CustomerEmail,
        status = order.Status.ToString(),
        order.PaymentIntentId,
        order.TotalAmount,
        order.Currency,
        order.PaidAtUtc,
        order.CreatedAtUtc
    };
}
