using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration
    : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(
        EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("ROLE_PERMISSIONS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.RoleId)
            .HasColumnName("ROLE_ID")
            .IsRequired();

        builder.Property(x => x.PermissionId)
            .HasColumnName("PERMISSION_ID")
            .IsRequired();

        builder.HasIndex(
                x => new
                {
                    x.RoleId,
                    x.PermissionId
                })
            .IsUnique();

        builder.HasOne<Role>()
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}