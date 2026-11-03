using System.Security.Cryptography;
using System.Text;
using CosmicWiki.Product.Api.Internal;
using CosmicWiki.Product.Application.Features.InternalProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Product.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("internal/product")]
public sealed class InternalProductController(
    ISender sender,
    IOptions<InternalProductOptions> options) : ControllerBase
{
    private const string ServiceKeyHeader = "X-Internal-Service-Key";

    [HttpGet("snapshots/{productId:guid}")]
    public async Task<ActionResult<ProductSnapshotResponse>> GetSnapshot(
        Guid productId,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
            return Unauthorized();

        var snapshot = await sender.Send(
            new GetProductSnapshotQuery(productId),
            cancellationToken);
        return snapshot is null ? NotFound() : Ok(snapshot);
    }

    [HttpPost("reservations")]
    public async Task<ActionResult<ReservationResult>> Reserve(
        [FromBody] ReserveInventoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
            return Unauthorized();

        var reserved = await sender.Send(
            new ReserveProductInventoryCommand(
                request.OrderId,
                request.Lines
                    .Select(line => new ReservationLine(line.ProductId, line.Quantity))
                    .ToArray()),
            cancellationToken);
        return Ok(new ReservationResult(reserved));
    }

    [HttpPost("reservations/{orderId:guid}/confirm")]
    public async Task<ActionResult<ReservationResult>> Confirm(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
            return Unauthorized();

        var confirmed = await sender.Send(
            new ConfirmProductInventoryCommand(orderId),
            cancellationToken);
        return Ok(new ReservationResult(confirmed));
    }

    [HttpDelete("reservations/{orderId:guid}")]
    public async Task<IActionResult> Release(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
            return Unauthorized();

        await sender.Send(
            new ReleaseProductInventoryCommand(orderId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("reservations/release-expired")]
    public async Task<ActionResult<ExpiredReservationsResult>> ReleaseExpired(
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
            return Unauthorized();

        var released = await sender.Send(
            new ReleaseExpiredProductInventoryCommand(),
            cancellationToken);
        return Ok(new ExpiredReservationsResult(released));
    }

    private bool IsAuthorized()
    {
        var configuredKey = options.Value.ServiceKey;
        if (string.IsNullOrWhiteSpace(configuredKey) ||
            !Request.Headers.TryGetValue(ServiceKeyHeader, out var suppliedKey))
            return false;

        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey.ToString());
        return configuredBytes.Length == suppliedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(configuredBytes, suppliedBytes);
    }
}

public sealed record ProductSnapshotResponse(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    string? ImageUrl,
    int AvailableStock,
    bool IsActive);

public sealed record ReserveInventoryRequest(
    Guid OrderId,
    IReadOnlyCollection<ReservationLineRequest> Lines);

public sealed record ReservationLineRequest(Guid ProductId, int Quantity);

public sealed record ReservationResult(bool Succeeded);

public sealed record ExpiredReservationsResult(int ReleasedCount);
