using CosmicWiki.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Product.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(
        EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("CATEGORIES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

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
            .HasMaxLength(1000);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}