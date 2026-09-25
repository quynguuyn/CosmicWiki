using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Visit.Infrastructure.Persistence.Configurations;

public sealed class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("FAQS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.Question).HasColumnName("QUESTION").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Answer).HasColumnName("ANSWER").HasMaxLength(10000).IsRequired();
        builder.Property(x => x.DisplayOrder).HasColumnName("DISPLAY_ORDER").IsRequired();
        builder.Property(x => x.IsPublished).HasColumnName("IS_PUBLISHED").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UPDATED_AT_UTC").IsRequired();
        builder.HasIndex(x => new { x.IsPublished, x.DisplayOrder });
    }
}
