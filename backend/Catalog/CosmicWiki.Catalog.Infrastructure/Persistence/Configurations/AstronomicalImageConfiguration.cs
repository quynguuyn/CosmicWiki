using System.Text.Json;
using CosmicWiki.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Configurations;

public sealed class AstronomicalImageConfiguration
    : IEntityTypeConfiguration<AstronomicalImage>
{
    public void Configure(
        EntityTypeBuilder<AstronomicalImage> builder)
    {
        builder.ToTable("ASTRONOMICAL_IMAGES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .IsRequired();

        builder.Property(x => x.AstronomicalObjectId)
            .HasColumnName("ASTRONOMICAL_OBJECT_ID")
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
                    (left == null && right == null) ||
                    (left != null &&
                     right != null &&
                     left.SequenceEqual(right)),

                urls =>
                    urls.Aggregate(
                        0,
                        (hash, url) =>
                            HashCode.Combine(hash, url.GetHashCode())),

                urls => urls.ToList());

        builder.Property(x => x.Urls)
            .HasColumnName("URLS")
            .HasColumnType("CLOB")
            .HasConversion(
                urlsConverter,
                urlsComparer)
            .IsRequired();
        
        builder.HasOne<AstronomicalObject>()
            .WithMany()
            .HasForeignKey(x => x.AstronomicalObjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => x.AstronomicalObjectId);
    }
}