using CosmicWiki.Product.Domain.Entities;
using CosmicWiki.Product.Infrastructure.Persistence.Outbox;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Product.Infrastructure.Persistence;

public sealed class ProductDbContext : DbContext
{
    public ProductDbContext(
        DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductEntity> Products =>
        Set<ProductEntity>();

    public DbSet<Category> Categories =>
        Set<Category>();

    public DbSet<ProductImage> ProductImages =>
        Set<ProductImage>();
    
    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    public DbSet<InventoryReservation> InventoryReservations =>
        Set<InventoryReservation>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(
            "COSMICWIKI_PRODUCT");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProductDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}