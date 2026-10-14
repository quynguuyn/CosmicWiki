using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Catalog.Infrastructure.Persistence.Configurations;

public sealed class AstronomicalObjectConfiguration
    : IEntityTypeConfiguration<AstronomicalObject>
{
    public void Configure(
        EntityTypeBuilder<AstronomicalObject> builder)
    {
        builder.ToTable("ASTRONOMICAL_OBJECTS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("NAME")
            .HasMaxLength(500);

        builder.Property(x => x.Kind)
            .HasColumnName("KIND")
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(AstronomicalObjectKind.Other)
            .IsRequired();

        builder.Property(x => x.ObjectType)
            .HasColumnName("OBJECT_TYPE")
            .HasMaxLength(200);

        builder.Property(x => x.Catalog)
            .HasColumnName("CATALOG")
            .HasMaxLength(50);

        builder.Property(x => x.CatalogValue)
            .HasColumnName("CATALOG_VALUE")
            .HasMaxLength(200);

        builder.Property(x => x.RightAscensionDegrees)
            .HasColumnName("RA_DEGREES");

        builder.Property(x => x.DeclinationDegrees)
            .HasColumnName("DEC_DEGREES");

        builder.HasIndex(x => new { x.Catalog, x.CatalogValue })
            .IsUnique()
            .HasFilter(null);
    }
}