using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Visit.Infrastructure.Persistence.Configurations;

public sealed class ExhibitionConfiguration : IEntityTypeConfiguration<Exhibition>
{
    public void Configure(EntityTypeBuilder<Exhibition> builder)
    {
        builder.ToTable("EXHIBITIONS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.Number).HasColumnName("NUMBER").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Category).HasColumnName("CATEGORY").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Title).HasColumnName("TITLE").HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasColumnName("DESCRIPTION").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ImageUrl).HasColumnName("IMAGE_URL").HasMaxLength(2000);
        builder.Property(x => x.DisplayOrder).HasColumnName("DISPLAY_ORDER").IsRequired();
        builder.Property(x => x.IsPublished).HasColumnName("IS_PUBLISHED").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UPDATED_AT_UTC").IsRequired();
        builder.HasIndex(x => new { x.IsPublished, x.DisplayOrder });
    }
}
