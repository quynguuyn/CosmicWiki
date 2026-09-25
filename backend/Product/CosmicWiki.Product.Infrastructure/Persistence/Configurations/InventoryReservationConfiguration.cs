using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Product.Infrastructure.Persistence.Configurations;

public sealed class InventoryReservationConfiguration
    : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.ToTable("INVENTORY_RESERVATIONS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.OrderId).HasColumnName("ORDER_ID").IsRequired();
        builder.Property(x => x.ProductId).HasColumnName("PRODUCT_ID").IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("QUANTITY").IsRequired();
        builder.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.Property(x => x.ExpiresAtUtc).HasColumnName("EXPIRES_AT_UTC").IsRequired();
        builder.Property(x => x.ReleasedAtUtc).HasColumnName("RELEASED_AT_UTC");
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
    }
}
