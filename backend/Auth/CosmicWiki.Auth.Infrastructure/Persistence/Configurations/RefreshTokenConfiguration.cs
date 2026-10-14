using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("REFRESH_TOKENS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("USER_ID")
            .IsRequired();

        builder.Property(x => x.FamilyId)
            .HasColumnName("FAMILY_ID")
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("TOKEN_HASH")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("CREATED_AT_UTC")
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .HasColumnName("EXPIRES_AT_UTC")
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc)
            .HasColumnName("REVOKED_AT_UTC")
            .IsConcurrencyToken();

        builder.Property(x => x.ReplacedByTokenHash)
            .HasColumnName("REPLACED_BY_TOKEN_HASH")
            .HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.FamilyId });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
