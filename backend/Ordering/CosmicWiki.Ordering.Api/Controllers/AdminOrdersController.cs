using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Application.Features.Orders;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = Permissions.ShippingWrite)]
public sealed class AdminOrdersController(
    ISender sender) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var startOfDayUtc = DateTime.UtcNow.Date;
        var today = await sender.Send(new GetTodayOrdersQuery(startOfDayUtc), cancellationToken);
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
        var result = await sender.Send(
            new GetTodayOrdersPageQuery(startOfDayUtc, parsedStatus, (page - 1) * pageSize, pageSize),
            cancellationToken);
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return Ok(new
        {
            items = result.Items.Select(ToResponse),
            page,
            pageSize,
            totalCount = result.TotalCount,
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

        try
        {
            var order = await sender.Send(
                new UpdateOrderStatusCommand(orderId, nextStatus),
                cancellationToken);
            if (order is null)
                return NotFound();
            return Ok(ToResponse(order));
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
}
