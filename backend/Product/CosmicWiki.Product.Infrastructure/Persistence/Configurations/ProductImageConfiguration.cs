using System.Text.Json;
using CosmicWiki.Product.Domain.Entities;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        var urlsConverter =
            new ValueConverter<IReadOnlyCollection<string>, string>(
                urls => JsonSerializer.Serialize(urls, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(
                            json,
                            (JsonSerializerOptions?)null)
                        ?? new List<string>());

        var urlsComparer =
            new ValueComparer<IReadOnlyCollection<string>>(
                (left, right) =>
                    left != null &&
                    right != null &&
                    left.SequenceEqual(right),
                urls => urls.Aggregate(
                    0,
                    (hash, url) => HashCode.Combine(hash, url.GetHashCode())),
                urls => urls.ToList());

        builder.Property(x => x.Urls)
            .HasColumnName("URLS")
            .HasColumnType("CLOB")
            .HasConversion(urlsConverter, urlsComparer)
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