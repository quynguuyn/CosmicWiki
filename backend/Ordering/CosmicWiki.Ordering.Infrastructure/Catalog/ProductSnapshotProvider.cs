using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CosmicWiki.Ordering.Infrastructure.Catalog;

public sealed class ProductSnapshotProvider(IConfiguration configuration)
    : IProductSnapshotProvider
{
    public async Task<ProductSnapshot?> GetAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("ProductDatabase")
            ?? throw new InvalidOperationException(
                "ProductDatabase connection string is not configured.");

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p."ID", p."SKU", p."NAME", p."SALE_PRICE", p."STOCK_QUANTITY", p."STATUS",
                   (SELECT pi."URL"
                    FROM "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" pi
                    WHERE pi."PRODUCT_ID" = p."ID"
                    ORDER BY pi."DISPLAY_ORDER"
                    FETCH FIRST 1 ROWS ONLY) AS "IMAGE_URL"
            FROM "COSMICWIKI_PRODUCT"."PRODUCTS" p
            WHERE p."ID" = :productId
            """;
        command.Parameters.Add("productId", OracleDbType.Raw, productId.ToByteArray(),
            System.Data.ParameterDirection.Input);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new ProductSnapshot(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDecimal(3),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.GetInt32(4),
            reader.GetInt32(5) == 2);
    }
}
