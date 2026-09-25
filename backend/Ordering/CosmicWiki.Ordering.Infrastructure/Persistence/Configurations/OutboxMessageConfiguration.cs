using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OUTBOX_MESSAGES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Topic).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(4000);
        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => x.PublishedAtUtc);
    }
}
