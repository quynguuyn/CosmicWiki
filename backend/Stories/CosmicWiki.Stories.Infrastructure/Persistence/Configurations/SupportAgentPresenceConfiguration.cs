using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmicWiki.Stories.Infrastructure.Persistence.Configurations;

public sealed class SupportAgentPresenceConfiguration : IEntityTypeConfiguration<SupportAgentPresence>
{
    public void Configure(EntityTypeBuilder<SupportAgentPresence> builder)
    {
        builder.ToTable("SUPPORT_AGENT_PRESENCE");
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.UserId).HasColumnName("USER_ID").ValueGeneratedNever();
        builder.Property(x => x.DisplayName).HasColumnName("DISPLAY_NAME").HasMaxLength(256);
        builder.Property(x => x.AcceptingChats).HasColumnName("ACCEPTING_CHATS").IsRequired();
        builder.Property(x => x.LastSeenAtUtc).HasColumnName("LAST_SEEN_AT_UTC").IsRequired();
        builder.Property(x => x.LastAssignedAtUtc).HasColumnName("LAST_ASSIGNED_AT_UTC");
    }
}
