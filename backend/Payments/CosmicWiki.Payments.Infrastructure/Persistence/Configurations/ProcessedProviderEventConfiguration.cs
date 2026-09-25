using CosmicWiki.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Payments.Infrastructure.Persistence.Configurations;

public sealed class ProcessedProviderEventConfiguration : IEntityTypeConfiguration<ProcessedProviderEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedProviderEvent> builder)
    {
        builder.ToTable("PROCESSED_PROVIDER_EVENTS");
        builder.HasKey(x => x.ProviderEventId);
        builder.Property(x => x.ProviderEventId).HasMaxLength(200);
    }
}
