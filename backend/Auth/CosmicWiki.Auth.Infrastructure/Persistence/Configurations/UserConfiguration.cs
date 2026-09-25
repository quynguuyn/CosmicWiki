using CosmicWiki.Auth.Domain.Entities;
using CosmicWiki.Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration
    : IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USERS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedNever();

        builder.Property(x => x.Username)
            .HasColumnName("USERNAME")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion(
                username => username.Value,
                value => Username.Create(value));

        builder.Property(x => x.Email)
            .HasColumnName("EMAIL")
            .HasMaxLength(254)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => Email.Create(value));

        builder.Property(x => x.PasswordHash)
            .HasColumnName("PASSWORD_HASH")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("STATUS")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}