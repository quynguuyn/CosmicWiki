using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Stories.Infrastructure.Persistence.Configurations;

public sealed class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable("STORIES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.Category).HasColumnName("CATEGORY").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Era).HasColumnName("ERA").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasColumnName("TITLE").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Slug).HasColumnName("SLUG").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasColumnName("DESCRIPTION").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Content).HasColumnName("CONTENT").HasMaxLength(20000).IsRequired();
        builder.Property(x => x.Author).HasColumnName("AUTHOR").HasMaxLength(300).IsRequired();
        builder.Property(x => x.ReadTimeMinutes).HasColumnName("READ_TIME_MINUTES").IsRequired();
        builder.Property(x => x.ImageUrl).HasColumnName("IMAGE_URL").HasMaxLength(2000);
        builder.Property(x => x.ImageAlt).HasColumnName("IMAGE_ALT").HasMaxLength(1000);
        builder.Property(x => x.Status).HasColumnName("STATUS").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UPDATED_AT_UTC").IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
