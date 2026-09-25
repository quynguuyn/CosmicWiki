using CosmicWiki.Catalog.Domain.Entities;
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
    }
}