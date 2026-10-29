using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Ordering.Application.Events;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = Permissions.ShippingWrite)]
public sealed class AdminOrdersController(
    IOrderRepository orders,
    IInventoryReservationService inventory,
    IOrderEmailSender emailSender,
    IOrderNotificationPublisher notificationPublisher) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var startOfDayUtc = DateTime.UtcNow.Date;
        var today = await orders.GetCreatedSinceAsync(startOfDayUtc, cancellationToken);
        return Ok(today.Select(ToResponse));
    }

    [HttpGet("today/page")]
    public async Task<IActionResult> GetTodayPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than zero.");
        if (status is not null && !Enum.TryParse<OrderStatus>(status, true, out _))
            return BadRequest("A valid order status is required.");

        pageSize = Math.Clamp(pageSize, 1, 100);
        OrderStatus? parsedStatus = status is null
            ? null
            : Enum.Parse<OrderStatus>(status, true);
        var startOfDayUtc = DateTime.UtcNow.Date;
        var (ordersPage, totalCount) = await orders.GetCreatedSincePageAsync(
            startOfDayUtc,
            parsedStatus,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            items = ordersPage.Select(ToResponse),
            page,
            pageSize,
            totalCount,
            totalPages,
            hasPreviousPage = page > 1,
            hasNextPage = page < totalPages
        });
    }

    [HttpPatch("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest? request,
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
            {
                if (nextStatus == OrderStatus.Cancelled)
                    await inventory.ReleaseAsync(order.Id, cancellationToken);
                return Ok(ToResponse(order));
            }

            await orders.SaveChangesAsync(cancellationToken);
            await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
            if (nextStatus == OrderStatus.Cancelled)
                await inventory.ReleaseAsync(order.Id, cancellationToken);
            await emailSender.SendOrderStatusChangedAsync(
                order.Id, order.CustomerEmail, order.TotalAmount, order.Currency,
                order.Status.ToString(), ToEmailLines(order), cancellationToken);
            return Ok(ToResponse(order));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "The order status changed. Refresh and try again." });
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
        order.CreatedAtUtc,
        order.DeliveryAddress,
        lines = order.Lines.Select(line => new
        {
            line.ProductId,
            line.SKU,
            line.ProductName,
            line.UnitPrice,
            line.ProductImageUrl,
            line.Quantity,
            lineTotal = line.LineTotal
        })
    };

    private static IReadOnlyCollection<OrderCreatedLine> ToEmailLines(Domain.Entities.Order order) =>
        order.Lines.Select(line => new OrderCreatedLine(
            line.SKU,
            line.ProductName,
            line.UnitPrice,
            line.Quantity,
            line.LineTotal)).ToArray();
}
