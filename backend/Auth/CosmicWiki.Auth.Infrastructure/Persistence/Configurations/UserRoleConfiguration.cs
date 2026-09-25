using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration
    : IEntityTypeConfiguration<UserRole>
{
    public void Configure(
        EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("USER_ROLES");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("USER_ID")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("ROLE_ID")
            .IsRequired();

        builder.HasIndex(
                x => new
                {
                    x.UserId,
                    x.RoleId
                })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}