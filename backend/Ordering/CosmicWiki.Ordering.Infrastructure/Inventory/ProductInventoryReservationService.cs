using CosmicWiki.Caching;
using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace CosmicWiki.Ordering.Infrastructure.Inventory;

public sealed class ProductInventoryReservationService(
    IConfiguration configuration,
    IProductCache productCache)
    : IInventoryReservationService
{
    private static readonly TimeSpan ReservationTtl = TimeSpan.FromMinutes(15);

    public async Task<bool> ReserveAsync(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationLine> lines,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        if (lines.Count == 0)
            throw new ArgumentException("Reservation lines are required.", nameof(lines));

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            await using (var existingCommand = connection.CreateCommand())
            {
                existingCommand.Transaction = transaction;
                existingCommand.CommandText = """
                    SELECT COUNT(*)
                    FROM "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                    WHERE "ORDER_ID" = :orderId AND "STATUS" = 'ACTIVE'
                    """;
                existingCommand.Parameters.Add("orderId", OracleDbType.Raw).Value = orderId.ToByteArray();
                var existing = Convert.ToInt32(await existingCommand.ExecuteScalarAsync(cancellationToken));
                if (existing > 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }
            }

            foreach (var line in lines)
            {
                await using var updateCommand = connection.CreateCommand();
                updateCommand.Transaction = transaction;
                updateCommand.CommandText = """
                    UPDATE "COSMICWIKI_PRODUCT"."PRODUCTS"
                    SET "STOCK_QUANTITY" = "STOCK_QUANTITY" - :quantity
                    WHERE "ID" = :productId
                      AND "STATUS" = 2
                      AND "STOCK_QUANTITY" >= :quantity
                    """;
                updateCommand.Parameters.Add("quantity", OracleDbType.Int32).Value = line.Quantity;
                updateCommand.Parameters.Add("productId", OracleDbType.Raw).Value = line.ProductId.ToByteArray();

                if (await updateCommand.ExecuteNonQueryAsync(cancellationToken) != 1)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }
            }

            foreach (var line in lines)
            {
                await using var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = """
                    INSERT INTO "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                        ("ID", "ORDER_ID", "PRODUCT_ID", "QUANTITY", "STATUS", "CREATED_AT_UTC", "EXPIRES_AT_UTC")
                    VALUES (:id, :orderId, :productId, :quantity, 'ACTIVE', :createdAtUtc, :expiresAtUtc)
                    """;
                insertCommand.Parameters.Add("id", OracleDbType.Raw).Value = Guid.NewGuid().ToByteArray();
                insertCommand.Parameters.Add("orderId", OracleDbType.Raw).Value = orderId.ToByteArray();
                insertCommand.Parameters.Add("productId", OracleDbType.Raw).Value = line.ProductId.ToByteArray();
                insertCommand.Parameters.Add("quantity", OracleDbType.Int32).Value = line.Quantity;
                insertCommand.Parameters.Add("createdAtUtc", OracleDbType.TimeStamp).Value = DateTime.UtcNow;
                insertCommand.Parameters.Add("expiresAtUtc", OracleDbType.TimeStamp).Value = DateTime.UtcNow.Add(ReservationTtl);
                await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            foreach (var line in lines)
            {
                await productCache.InvalidateAsync(line.ProductId, CancellationToken.None);
            }

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<bool> ConfirmAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await using var statusCommand = connection.CreateCommand();
            statusCommand.Transaction = transaction;
            statusCommand.CommandText = """
                SELECT COUNT(*)
                FROM "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                WHERE "ORDER_ID" = :orderId AND "STATUS" = 'ACTIVE'
                """;
            statusCommand.Parameters.Add("orderId", OracleDbType.Raw).Value = orderId.ToByteArray();
            var activeCount = Convert.ToInt32(await statusCommand.ExecuteScalarAsync(cancellationToken));

            if (activeCount == 0)
            {
                statusCommand.Parameters.Clear();
                statusCommand.CommandText = """
                    SELECT COUNT(*)
                    FROM "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                    WHERE "ORDER_ID" = :orderId AND "STATUS" = 'CONFIRMED'
                    """;
                statusCommand.Parameters.Add("orderId", OracleDbType.Raw).Value = orderId.ToByteArray();
                var confirmedCount = Convert.ToInt32(await statusCommand.ExecuteScalarAsync(cancellationToken));
                await transaction.CommitAsync(cancellationToken);
                return confirmedCount > 0;
            }

            await SetStatusAsync(connection, transaction, orderId, "CONFIRMED", null, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task ReleaseAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            await using var selectCommand = connection.CreateCommand();
            selectCommand.Transaction = transaction;
            selectCommand.CommandText = """
                SELECT "PRODUCT_ID", "QUANTITY"
                FROM "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                WHERE "ORDER_ID" = :orderId AND "STATUS" = 'ACTIVE'
                FOR UPDATE
                """;
            selectCommand.Parameters.Add("orderId", OracleDbType.Raw).Value = orderId.ToByteArray();

            var lines = new List<(byte[] ProductId, int Quantity)>();
            await using (var reader = await selectCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    lines.Add(((byte[])reader.GetValue(0), reader.GetInt32(1)));
            }

            foreach (var line in lines)
            {
                await using var updateProductCommand = connection.CreateCommand();
                updateProductCommand.Transaction = transaction;
                updateProductCommand.CommandText = """
                    UPDATE "COSMICWIKI_PRODUCT"."PRODUCTS"
                    SET "STOCK_QUANTITY" = "STOCK_QUANTITY" + :quantity
                    WHERE "ID" = :productId
                    """;
                updateProductCommand.Parameters.Add("quantity", OracleDbType.Int32).Value = line.Quantity;
                updateProductCommand.Parameters.Add("productId", OracleDbType.Raw).Value = line.ProductId;
                await updateProductCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await SetStatusAsync(connection, transaction, orderId, "RELEASED", DateTime.UtcNow, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            foreach (var line in lines)
            {
                await productCache.InvalidateAsync(new Guid(line.ProductId), CancellationToken.None);
            }
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<int> ReleaseExpiredAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await using var selectCommand = connection.CreateCommand();
            selectCommand.Transaction = transaction;
            selectCommand.CommandText = """
                SELECT "ORDER_ID"
                FROM "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                WHERE "STATUS" = 'ACTIVE' AND "EXPIRES_AT_UTC" <= :nowUtc
                FOR UPDATE
                """;
            selectCommand.Parameters.Add("nowUtc", OracleDbType.TimeStamp).Value = DateTime.UtcNow;

            var orderIds = new HashSet<Guid>();
            await using (var reader = await selectCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    orderIds.Add(new Guid((byte[])reader.GetValue(0)));
            }

            foreach (var expiredOrderId in orderIds)
            {
                await using var releaseCommand = connection.CreateCommand();
                releaseCommand.Transaction = transaction;
                releaseCommand.CommandText = """
                    SELECT "PRODUCT_ID", "QUANTITY"
                    FROM "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
                    WHERE "ORDER_ID" = :orderId AND "STATUS" = 'ACTIVE'
                    """;
                releaseCommand.Parameters.Add("orderId", OracleDbType.Raw).Value = expiredOrderId.ToByteArray();

                var lines = new List<(byte[] ProductId, int Quantity)>();
                await using (var releaseReader = await releaseCommand.ExecuteReaderAsync(cancellationToken))
                {
                    while (await releaseReader.ReadAsync(cancellationToken))
                        lines.Add(((byte[])releaseReader.GetValue(0), releaseReader.GetInt32(1)));
                }

                foreach (var line in lines)
                {
                    await using var updateProductCommand = connection.CreateCommand();
                    updateProductCommand.Transaction = transaction;
                    updateProductCommand.CommandText = """
                        UPDATE "COSMICWIKI_PRODUCT"."PRODUCTS"
                        SET "STOCK_QUANTITY" = "STOCK_QUANTITY" + :quantity
                        WHERE "ID" = :productId
                        """;
                    updateProductCommand.Parameters.Add("quantity", OracleDbType.Int32).Value = line.Quantity;
                    updateProductCommand.Parameters.Add("productId", OracleDbType.Raw).Value = line.ProductId;
                    await updateProductCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await SetStatusAsync(connection, transaction, expiredOrderId, "RELEASED", DateTime.UtcNow, cancellationToken);

                foreach (var line in lines)
                {
                    await productCache.InvalidateAsync(new Guid(line.ProductId), CancellationToken.None);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return orderIds.Count;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task SetStatusAsync(Guid orderId, string status, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await SetStatusAsync(connection, transaction, orderId, status, null, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task SetStatusAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        Guid orderId,
        string status,
        DateTime? releasedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE "COSMICWIKI_PRODUCT"."INVENTORY_RESERVATIONS"
            SET "STATUS" = :status, "RELEASED_AT_UTC" = :releasedAtUtc
            WHERE "ORDER_ID" = :orderId AND "STATUS" = 'ACTIVE'
            """;
        command.Parameters.Add("status", OracleDbType.Varchar2).Value = status;
        command.Parameters.Add("releasedAtUtc", OracleDbType.TimeStamp).Value = releasedAtUtc ?? (object)DBNull.Value;
        command.Parameters.Add("orderId", OracleDbType.Raw).Value = orderId.ToByteArray();
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<OracleConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("ProductDatabase")
            ?? throw new InvalidOperationException("ProductDatabase connection string is not configured.");
        var connection = new OracleConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
