using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext : DbContext, IUnitOfWork
{
    public AuthDbContext(
        DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuthDbContext).Assembly);
    }
}