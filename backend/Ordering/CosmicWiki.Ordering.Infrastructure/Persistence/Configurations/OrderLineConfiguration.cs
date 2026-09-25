using CosmicWiki.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("ORDER_LINES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SKU).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Ignore(x => x.LineTotal);
    }
}
