using CosmicWiki.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("PAYMENTS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentIntentId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(x => x.ReconciledAtUtc);
        builder.HasIndex(x => x.PaymentIntentId).IsUnique();
    }
}
