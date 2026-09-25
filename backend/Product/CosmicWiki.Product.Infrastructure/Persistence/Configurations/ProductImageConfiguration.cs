using CosmicWiki.Product.Domain.Entities;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Product.Infrastructure.Persistence.Configurations;

public sealed class ProductImageConfiguration
    : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(
        EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("PRODUCT_IMAGES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.ProductId)
            .HasColumnName("PRODUCT_ID")
            .IsRequired();

        builder.Property(x => x.Url)
            .HasColumnName("URL")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .HasColumnName("DISPLAY_ORDER")
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.DisplayOrder
        });

        builder.HasOne<ProductEntity>()
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}