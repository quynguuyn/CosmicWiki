using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Visit.Infrastructure.Persistence.Configurations;

public sealed class VisitEventConfiguration : IEntityTypeConfiguration<VisitEvent>
{
    public void Configure(EntityTypeBuilder<VisitEvent> builder)
    {
        builder.ToTable("EVENTS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.EventDateUtc).HasColumnName("EVENT_DATE_UTC").IsRequired();
        builder.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasColumnName("DESCRIPTION").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Specification).HasColumnName("SPECIFICATION").HasMaxLength(500);
        builder.Property(x => x.Location).HasColumnName("LOCATION").HasMaxLength(500);
        builder.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(100).IsRequired();
        builder.Property(x => x.StatusType).HasColumnName("STATUS_TYPE").HasMaxLength(50);
        builder.Property(x => x.Action).HasColumnName("ACTION").HasMaxLength(100);
        builder.Property(x => x.DisplayOrder).HasColumnName("DISPLAY_ORDER").IsRequired();
        builder.Property(x => x.IsPublished).HasColumnName("IS_PUBLISHED").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UPDATED_AT_UTC").IsRequired();
        builder.HasIndex(x => new { x.IsPublished, x.EventDateUtc });
    }
}
