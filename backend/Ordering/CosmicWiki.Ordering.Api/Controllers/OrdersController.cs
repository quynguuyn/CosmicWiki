using CosmicWiki.Ordering.Application.Features.CreateOrder;
using CosmicWiki.Ordering.Application.Abstractions;
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
    ISender sender,
    IOrderRepository orders) : ControllerBase
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
        if (!Guid.TryParse(subject, out var authenticatedCustomerId) ||
            authenticatedCustomerId != request.CustomerId)
            return Forbid();

        var result = await sender.Send(
            new CreateOrderCommand(request.CustomerId, request.Currency, idempotencyKey, request.Lines),
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
            order.TotalAmount,
            order.Currency,
            order.PaidAtUtc
        });
    }
}

public sealed record CreateOrderRequest(
    Guid CustomerId,
    string Currency,
    IReadOnlyCollection<CreateOrderLineRequest> Lines);
