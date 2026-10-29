using CosmicWiki.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryEntryConfiguration
    : IEntityTypeConfiguration<OrderStatusHistoryEntry>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistoryEntry> builder)
    {
        builder.ToTable("ORDER_STATUS_HISTORY");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.ChangedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.OrderId, x.ChangedAtUtc });
    }
}
