using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Stories.Infrastructure.Persistence.Configurations;

public sealed class SupportConversationConfiguration : IEntityTypeConfiguration<SupportConversation>
{
    public void Configure(EntityTypeBuilder<SupportConversation> builder)
    {
        builder.ToTable("SUPPORT_CONVERSATIONS");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
        builder.Property(x => x.CustomerUserId).HasColumnName("CUSTOMER_USER_ID");
        builder.Property(x => x.CustomerDisplayName).HasColumnName("CUSTOMER_DISPLAY_NAME").HasMaxLength(256);
        builder.Property(x => x.GuestTokenHash).HasColumnName("GUEST_TOKEN_HASH").HasMaxLength(64);
        builder.Property(x => x.AssignedSupportId).HasColumnName("ASSIGNED_SUPPORT_ID");
        builder.Property(x => x.AssignedSupportDisplayName).HasColumnName("ASSIGNED_SUPPORT_DISPLAY_NAME").HasMaxLength(256);
        builder.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UPDATED_AT_UTC").IsRequired();
        builder.Property(x => x.LastMessageAtUtc).HasColumnName("LAST_MESSAGE_AT_UTC");
        builder.HasMany(x => x.Messages).WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.GuestTokenHash).IsUnique().HasFilter(null);
        builder.HasIndex(x => new { x.CustomerUserId, x.Status });
        builder.HasIndex(x => new { x.AssignedSupportId, x.Status });
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
    }
}
