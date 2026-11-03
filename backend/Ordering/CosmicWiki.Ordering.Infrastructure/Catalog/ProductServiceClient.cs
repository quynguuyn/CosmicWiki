using System.Net;
using System.Net.Http.Json;
using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Infrastructure.Catalog;

public sealed class ProductServiceClient(
    HttpClient httpClient,
    IOptions<ProductServiceOptions> options)
    : IProductSnapshotProvider, IInventoryReservationService
{
    private const string ServiceKeyHeader = "X-Internal-Service-Key";

    public async Task<ProductSnapshot?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"snapshots/{productId:D}");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductSnapshotContract>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Product service returned an empty snapshot.");
        return new ProductSnapshot(
            product.ProductId,
            product.Sku,
            product.Name,
            product.UnitPrice,
            product.ImageUrl,
            product.AvailableStock,
            product.IsActive);
    }

    public async Task<bool> ReserveAsync(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationLine> lines,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, "reservations");
        request.Content = JsonContent.Create(new ReserveRequest(
            orderId,
            lines.Select(line => new ReservationLineContract(
                line.ProductId,
                line.Quantity)).ToArray()));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ReservationResultContract>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Product service returned an empty reservation result.");
        return result.Succeeded;
    }

    public async Task<bool> ConfirmAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            $"reservations/{orderId:D}/confirm");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ReservationResultContract>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Product service returned an empty confirmation result.");
        return result.Succeeded;
    }

    public async Task ReleaseAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Delete,
            $"reservations/{orderId:D}");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int> ReleaseExpiredAsync(CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            "reservations/release-expired");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExpiredReservationsResultContract>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Product service returned an empty expiry result.");
        return result.ReleasedCount;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri)
    {
        var serviceKey = options.Value.ServiceKey;
        if (string.IsNullOrWhiteSpace(serviceKey) || serviceKey == "[SECRET_FROM_ENV]")
            throw new InvalidOperationException("ProductService:ServiceKey is not configured.");

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add(ServiceKeyHeader, serviceKey);
        return request;
    }

    private sealed record ProductSnapshotContract(
        Guid ProductId,
        string Sku,
        string Name,
        decimal UnitPrice,
        string? ImageUrl,
        int AvailableStock,
        bool IsActive);

    private sealed record ReserveRequest(
        Guid OrderId,
        IReadOnlyCollection<ReservationLineContract> Lines);

    private sealed record ReservationLineContract(Guid ProductId, int Quantity);

    private sealed record ReservationResultContract(bool Succeeded);

    private sealed record ExpiredReservationsResultContract(int ReleasedCount);
}
