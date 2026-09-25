using CosmicWiki.Product.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Product.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(
        EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OUTBOX_MESSAGES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID");

        builder.Property(x => x.EventId)
            .HasColumnName("EVENT_ID")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("AGGREGATE_ID")
            .IsRequired();

        builder.Property(x => x.Topic)
            .HasColumnName("TOPIC")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("TYPE")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("PAYLOAD")
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .HasColumnName("OCCURRED_AT_UTC")
            .IsRequired();

        builder.Property(x => x.PublishedAtUtc)
            .HasColumnName("PUBLISHED_AT_UTC");

        builder.Property(x => x.Attempts)
            .HasColumnName("ATTEMPTS")
            .IsRequired();

        builder.Property(x => x.Error)
            .HasColumnName("ERROR")
            .HasMaxLength(4000);

        builder.HasIndex(x => x.EventId)
            .IsUnique();

        builder.HasIndex(x => x.PublishedAtUtc);
    }
}