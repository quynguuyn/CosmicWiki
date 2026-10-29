using CosmicWiki.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("ORDERS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.CustomerEmail).HasMaxLength(320);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DeliveryAddress).HasColumnName("DELIVERY_ADDRESS").HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired().IsConcurrencyToken();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.PaymentIntentId).HasMaxLength(200);
        builder.Property(x => x.ClientSecret).HasMaxLength(200);
        builder.Property(x => x.DeliveryPersonId).HasColumnName("DELIVERY_PERSON_ID");
        builder.Property(x => x.DeliveryPersonName).HasColumnName("DELIVERY_PERSON_NAME").HasMaxLength(100);
        builder.Property(x => x.DeliveryLatitude).HasColumnName("DELIVERY_LATITUDE");
        builder.Property(x => x.DeliveryLongitude).HasColumnName("DELIVERY_LONGITUDE");
        builder.Property(x => x.DeliveryLocationUpdatedAtUtc).HasColumnName("DELIVERY_LOCATION_UPDATED_AT_UTC");
        builder.HasIndex(x => new { x.DeliveryPersonId, x.Status });
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
