using CosmicWiki.Visit.Application.Abstractions;
using CosmicWiki.Visit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Visit.Infrastructure.Persistence;

public sealed class VisitDbContext(DbContextOptions<VisitDbContext> options) : DbContext(options), IVisitUnitOfWork
{
    public DbSet<VisitRequest> VisitRequests => Set<VisitRequest>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("COSMICWIKI_VISIT");
        var entity = modelBuilder.Entity<VisitRequest>();
        entity.ToTable("VISIT_REQUESTS");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReferenceCode).HasMaxLength(16).IsRequired();
        entity.HasIndex(x => x.ReferenceCode).IsUnique();
        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
        entity.Property(x => x.Notes).HasMaxLength(2000);
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
    }
}
