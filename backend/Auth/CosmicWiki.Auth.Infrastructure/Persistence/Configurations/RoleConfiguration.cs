using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration
    : IEntityTypeConfiguration<Role>
{
    public void Configure(
        EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("ROLES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("NAME")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("DESCRIPTION")
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}