using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration
    : IEntityTypeConfiguration<Permission>
{
    public void Configure(
        EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("PERMISSIONS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasColumnName("CODE")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("NAME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("DESCRIPTION")
            .HasMaxLength(500);

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}