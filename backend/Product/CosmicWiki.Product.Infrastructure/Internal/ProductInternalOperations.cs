using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions;
using CosmicWiki.Product.Application.Features.InternalProduct;
using CosmicWiki.Product.Domain.Enums;
using CosmicWiki.Product.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Product.Infrastructure.Internal;

public sealed class ProductInternalOperations(
    ProductDbContext dbContext,
    IProductCache productCache) : IProductInternalOperations
{
    private static readonly TimeSpan ReservationTtl = TimeSpan.FromMinutes(15);

    public async Task<ProductSnapshotResult?> GetSnapshotAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == productId)
            .Select(x => new
            {
                x.Id,
                x.SKU,
                x.Name,
                x.SalePrice,
                x.StockQuantity,
                x.Status,
                ImageUrl = x.Images
                    .OrderBy(image => image.DisplayOrder)
                    .Select(image => image.Url)
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return product is null
            ? null
            : new ProductSnapshotResult(
                product.Id,
                product.SKU,
                product.Name,
                product.SalePrice,
                product.ImageUrl,
                product.StockQuantity,
                product.Status == ProductStatus.Active);
    }

    public async Task<bool> ReserveAsync(
        Guid orderId,
        IReadOnlyCollection<ReservationLine> lines,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        if (lines.Count == 0 || lines.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0))
            throw new ArgumentException("Valid reservation lines are required.", nameof(lines));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken);
        try
        {
            var existing = await dbContext.InventoryReservations
                .Where(x => x.OrderId == orderId)
                .Select(x => x.Status)
                .ToListAsync(cancellationToken);
            if (existing.Count > 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return existing.Any(status => status is "ACTIVE" or "CONFIRMED");
            }

            var now = DateTime.UtcNow;
            foreach (var line in lines)
            {
                var updated = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                     UPDATE "COSMICWIKI_PRODUCT"."PRODUCTS"
                     SET "STOCK_QUANTITY" = "STOCK_QUANTITY" - {line.Quantity}
                     WHERE "ID" = {line.ProductId}
                       AND "STATUS" = {(int)ProductStatus.Active}
                       AND "STOCK_QUANTITY" >= {line.Quantity}
                     """,
                    cancellationToken);
                if (updated != 1)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }

                dbContext.InventoryReservations.Add(new InventoryReservation
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    Status = "ACTIVE",
                    CreatedAtUtc = now,
                    ExpiresAtUtc = now.Add(ReservationTtl)
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            foreach (var line in lines)
                await productCache.InvalidateAsync(line.ProductId, CancellationToken.None);
            return true;
        }
        catch
        {
            await RollbackQuietlyAsync(transaction);
            throw;
        }
    }

    public async Task<bool> ConfirmAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken);
        try
        {
            var active = await dbContext.InventoryReservations
                .Where(x => x.OrderId == orderId && x.Status == "ACTIVE")
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.Status, "CONFIRMED"),
                    cancellationToken);
            if (active == 0)
            {
                var confirmed = await dbContext.InventoryReservations
                    .AnyAsync(
                        x => x.OrderId == orderId && x.Status == "CONFIRMED",
                        cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return confirmed;
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await RollbackQuietlyAsync(transaction);
            throw;
        }
    }

    public async Task ReleaseAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken);
        try
        {
            var reservations = await dbContext.InventoryReservations
                .AsNoTracking()
                .Where(x => x.OrderId == orderId && x.Status == "ACTIVE")
                .Select(x => new { x.Id, x.ProductId, x.Quantity })
                .ToListAsync(cancellationToken);

            var releasedAt = DateTime.UtcNow;
            foreach (var reservation in reservations)
            {
                var changed = await dbContext.InventoryReservations
                    .Where(x => x.Id == reservation.Id && x.Status == "ACTIVE")
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(x => x.Status, "RELEASED")
                            .SetProperty(x => x.ReleasedAtUtc, releasedAt),
                        cancellationToken);
                if (changed == 1)
                    await AddStockAsync(reservation.ProductId, reservation.Quantity, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            foreach (var reservation in reservations)
                await productCache.InvalidateAsync(reservation.ProductId, CancellationToken.None);
        }
        catch
        {
            await RollbackQuietlyAsync(transaction);
            throw;
        }
    }

    public async Task<int> ReleaseExpiredAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiredOrderIds = await dbContext.InventoryReservations
            .AsNoTracking()
            .Where(x => x.Status == "ACTIVE" && x.ExpiresAtUtc <= now)
            .Select(x => x.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var releasedOrders = 0;
        foreach (var orderId in expiredOrderIds)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationToken);
            try
            {
                var reservations = await dbContext.InventoryReservations
                    .AsNoTracking()
                    .Where(x => x.OrderId == orderId &&
                                x.Status == "ACTIVE" &&
                                x.ExpiresAtUtc <= now)
                    .Select(x => new { x.Id, x.ProductId, x.Quantity })
                    .ToListAsync(cancellationToken);
                var releasedCount = 0;
                foreach (var reservation in reservations)
                {
                    var changed = await dbContext.InventoryReservations
                        .Where(x => x.Id == reservation.Id &&
                                    x.Status == "ACTIVE" &&
                                    x.ExpiresAtUtc <= now)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(x => x.Status, "RELEASED")
                                .SetProperty(x => x.ReleasedAtUtc, now),
                            cancellationToken);
                    if (changed == 1)
                    {
                        releasedCount++;
                        await AddStockAsync(
                            reservation.ProductId,
                            reservation.Quantity,
                            cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                if (releasedCount > 0)
                {
                    releasedOrders++;
                    foreach (var reservation in reservations)
                        await productCache.InvalidateAsync(
                            reservation.ProductId,
                            CancellationToken.None);
                }
            }
            catch
            {
                await RollbackQuietlyAsync(transaction);
                throw;
            }
        }

        return releasedOrders;
    }

    private Task<int> AddStockAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE "COSMICWIKI_PRODUCT"."PRODUCTS"
             SET "STOCK_QUANTITY" = "STOCK_QUANTITY" + {quantity}
             WHERE "ID" = {productId}
             """,
            cancellationToken);
    }

    private static async Task RollbackQuietlyAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch
        {
        }
    }
}
