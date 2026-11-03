using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Stories.Infrastructure.AiChat;

public sealed class CosmicWikiProductCatalogClient(
    HttpClient httpClient,
    ILogger<CosmicWikiProductCatalogClient> logger)
{
    public async Task<string?> GetProductContextAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ProductPage>(
                "api/product/products?page=1&pageSize=50",
                cancellationToken);
            if (result?.Items is null)
                return null;

            var products = result.Items
                .Where(product =>
                    product.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
                    product.StockQuantity > 0)
                .Take(30)
                .Select(product =>
                {
                    var price = product.SalePrice > 0 && product.SalePrice < product.OriginalPrice
                        ? product.SalePrice
                        : product.OriginalPrice;
                    var description = string.IsNullOrWhiteSpace(product.Description)
                        ? string.Empty
                        : $"; mô tả: {product.Description.Trim()[..Math.Min(product.Description.Trim().Length, 240)]}";
                    return $"- {product.Name} (mã: {product.SKU}, giá: " +
                           $"{price.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} ₫, " +
                           $"còn hàng: {product.StockQuantity}, slug: {product.Slug}{description}";
                })
                .ToArray();

            return products.Length == 0 ? null : string.Join('\n', products);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "CosmicWiki product data is unavailable to AI chat.");
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "CosmicWiki product data returned an invalid response.");
            return null;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "CosmicWiki product data request timed out.");
            return null;
        }
    }

    private sealed record ProductPage(IReadOnlyCollection<ProductData>? Items);

    private sealed record ProductData(
        string SKU,
        string Name,
        string Slug,
        string? Description,
        decimal OriginalPrice,
        decimal SalePrice,
        int StockQuantity,
        string Status);
}
