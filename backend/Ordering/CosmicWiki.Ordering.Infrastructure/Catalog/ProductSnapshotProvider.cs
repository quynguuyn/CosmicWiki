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
            SELECT "ID", "SKU", "NAME", "SALE_PRICE", "STOCK_QUANTITY", "STATUS"
            FROM "COSMICWIKI_PRODUCT"."PRODUCTS"
            WHERE "ID" = :productId
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
            reader.GetInt32(4),
            reader.GetInt32(5) == 2);
    }
}
