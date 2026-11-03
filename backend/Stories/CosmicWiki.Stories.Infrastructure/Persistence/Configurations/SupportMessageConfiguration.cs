using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Stories.Infrastructure.Persistence.Configurations;

public sealed class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
{
    public void Configure(EntityTypeBuilder<SupportMessage> builder)
    {
        builder.ToTable("SUPPORT_MESSAGES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.ConversationId).HasColumnName("CONVERSATION_ID").IsRequired();
        builder.Property(x => x.SenderType).HasColumnName("SENDER_TYPE").HasMaxLength(20).IsRequired();
        builder.Property(x => x.SenderUserId).HasColumnName("SENDER_USER_ID");
        builder.Property(x => x.SenderDisplayName).HasColumnName("SENDER_DISPLAY_NAME").HasMaxLength(256);
        builder.Property(x => x.Body).HasColumnName("BODY").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.HasIndex(x => new { x.ConversationId, x.CreatedAtUtc });
    }
}
