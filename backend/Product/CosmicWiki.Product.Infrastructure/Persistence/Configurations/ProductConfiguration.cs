using CosmicWiki.Product.Domain.Entities;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Product.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(
        EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("PRODUCTS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.SKU)
            .HasColumnName("SKU")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("NAME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasColumnName("SLUG")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("DESCRIPTION")
            .HasMaxLength(4000);

        builder.Property(x => x.OriginalPrice)
            .HasColumnName("ORIGINAL_PRICE")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SalePrice)
            .HasColumnName("SALE_PRICE")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.StockQuantity)
            .HasColumnName("STOCK_QUANTITY")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("STATUS")
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("CATEGORY_ID")
            .IsRequired();

        builder.HasIndex(x => x.SKU)
            .IsUnique();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.CategoryId);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}