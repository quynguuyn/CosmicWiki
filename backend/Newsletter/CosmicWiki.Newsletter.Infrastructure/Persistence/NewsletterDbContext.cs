using CosmicWiki.Newsletter.Application.Abstractions;
using CosmicWiki.Newsletter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Newsletter.Infrastructure.Persistence;

public sealed class NewsletterDbContext(DbContextOptions<NewsletterDbContext> options)
    : DbContext(options), INewsletterUnitOfWork
{
    public DbSet<NewsletterSubscription> Subscriptions => Set<NewsletterSubscription>();
    public DbSet<NewsletterDelivery> Deliveries => Set<NewsletterDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("COSMICWIKI_NEWSLETTER");

        var subscription = modelBuilder.Entity<NewsletterSubscription>();
        subscription.ToTable("NEWSLETTER_SUBSCRIPTIONS");
        subscription.HasKey(x => x.Id);
        subscription.Property(x => x.Email).HasMaxLength(320).IsRequired();
        subscription.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        subscription.Property(x => x.ConfirmationTokenHash).HasMaxLength(128).IsRequired();
        subscription.Property(x => x.ConfirmationTokenExpiresAtUtc).IsRequired();
        subscription.Property(x => x.CreatedAtUtc).IsRequired();
        subscription.Property(x => x.UpdatedAtUtc).IsRequired();
        subscription.HasIndex(x => x.Email).IsUnique();
        subscription.HasIndex(x => x.ConfirmationTokenHash).IsUnique();

        var delivery = modelBuilder.Entity<NewsletterDelivery>();
        delivery.ToTable("NEWSLETTER_DELIVERIES");
        delivery.HasKey(x => x.Id);
        delivery.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        delivery.Property(x => x.RecipientEmail).HasMaxLength(320).IsRequired();
        delivery.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        delivery.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        delivery.Property(x => x.LastError).HasMaxLength(2000);
        delivery.Property(x => x.Attempts).IsRequired();
        delivery.Property(x => x.CreatedAtUtc).IsRequired();
        delivery.Property(x => x.UpdatedAtUtc).IsRequired();
        delivery.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        delivery.HasOne(x => x.Subscription)
            .WithMany(x => x.Deliveries)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
