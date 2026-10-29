using CosmicWiki.Discovery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Discovery.Infrastructure.Persistence.Configurations;

public sealed class SearchDocumentConfiguration
    : IEntityTypeConfiguration<SearchDocument>
{
    public void Configure(
        EntityTypeBuilder<SearchDocument> builder)
    {
        builder.ToTable("SEARCH_DOCUMENTS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID");

        builder.Property(x => x.SourceId)
            .HasColumnName("SOURCE_ID")
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasColumnName("SOURCE_TYPE")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("NAME")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("DESCRIPTION")
            .HasMaxLength(4000);

        builder.Property(x => x.ImageUrl)
            .HasColumnName("IMAGE_URL")
            .HasMaxLength(2000);

        builder.Property(x => x.Catalog)
            .HasColumnName("CATALOG")
            .HasMaxLength(100);

        builder.Property(x => x.CatalogValue)
            .HasColumnName("CATALOG_VALUE")
            .HasMaxLength(200);

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("UPDATED_AT")
            .IsRequired();

        builder.HasIndex(x => new { x.SourceType, x.SourceId })
            .IsUnique();

        builder.HasIndex(x => x.SourceType);

        builder.HasIndex(x => x.Name);

        builder.HasIndex(x => x.CatalogValue);
    }
}
