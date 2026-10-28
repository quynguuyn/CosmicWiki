using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PASSWORD_RESET_TOKENS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.UserId).HasColumnName("USER_ID").IsRequired();
        builder.Property(x => x.TokenHash).HasColumnName("TOKEN_HASH").HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).HasColumnName("EXPIRES_AT_UTC").IsRequired();
        builder.Property(x => x.UsedAtUtc).HasColumnName("USED_AT_UTC").IsConcurrencyToken();
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
