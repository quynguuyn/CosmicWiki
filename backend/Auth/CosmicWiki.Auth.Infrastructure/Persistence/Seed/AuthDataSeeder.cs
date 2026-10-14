using CosmicWiki.Auth.Domain.Entities;
using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.ValueObjects;
using CosmicWiki.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Seed;

public static class AuthDataSeeder
{
    public static async Task SeedAsync(
        AuthDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await SeedPermissionsAsync(
            dbContext,
            cancellationToken);

        await SeedRolesAsync(
            dbContext,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await SeedRolePermissionsAsync(
            dbContext,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await SeedConfiguredUserAsync(
            dbContext,
            passwordHasher,
            configuration,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await SeedUserRolesAsync(
            dbContext,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedConfiguredUserAsync(
        AuthDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var username = configuration["Auth:SeedUser:Username"];
        var email = configuration["Auth:SeedUser:Email"];
        var password = configuration["Auth:SeedUser:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
            return;

        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Username == Username.Create(username), cancellationToken);

        if (user is null)
        {
            user = User.Create(
                Username.Create(username),
                Email.Create(email),
                passwordHasher.Hash(password));
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var roles = await dbContext.Roles.ToListAsync(cancellationToken);
        foreach (var role in roles)
        {
            if (await dbContext.UserRoles.AnyAsync(
                    x => x.UserId == user.Id && x.RoleId == role.Id,
                    cancellationToken))
                continue;

            dbContext.UserRoles.Add(UserRole.Create(user.Id, role.Id));
        }
    }
    
    private static async Task SeedUserRolesAsync(
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userRole = await dbContext.Roles
            .SingleAsync(
                x => x.Name == "User",
                cancellationToken);

        var users = await dbContext.Users
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var exists = await dbContext.UserRoles
                .AnyAsync(
                    x =>
                        x.UserId == user.Id &&
                        x.RoleId == userRole.Id,
                    cancellationToken);

            if (exists)
                continue;

            dbContext.UserRoles.Add(
                UserRole.Create(
                    user.Id,
                    userRole.Id));
        }
    }

    private static async Task SeedPermissionsAsync(
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var permissions = new[]
        {
            (Permissions.AuthRead, "Read Auth"),
            (Permissions.AuthWrite, "Write Auth"),
            
            (Permissions.CatalogRead, "Read Catalog"),
            (Permissions.CatalogWrite, "Write Catalog"),

            (Permissions.ProductRead, "Read Product"),
            (Permissions.ProductWrite, "Write Product"),

            (Permissions.StoryRead, "Read Stories"),
            (Permissions.StoryWrite, "Write Stories"),

            (Permissions.DiscoveryRead, "Read Discovery"),
            (Permissions.DiscoveryWrite, "Write Discovery"),

            (Permissions.VisitRead, "Read Visit"),
            (Permissions.VisitWrite, "Write Visit"),

            (Permissions.OrderingWrite, "Create Orders"),
            (Permissions.ShippingWrite, "Update shipping statuses")
        };

        foreach (var (code, name) in permissions)
        {
            var exists = await dbContext.Permissions
                .AnyAsync(
                    x => x.Code == code,
                    cancellationToken);

            if (exists)
                continue;

            dbContext.Permissions.Add(
                Permission.Create(
                    code,
                    name));
        }
    }

    private static async Task SeedRolesAsync(
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var roles = new[]
        {
            ("Admin", "System administrator"),
            ("User", "Standard user")
        };

        foreach (var (name, description) in roles)
        {
            var exists = await dbContext.Roles
                .AnyAsync(
                    x => x.Name == name,
                    cancellationToken);

            if (exists)
                continue;

            dbContext.Roles.Add(
                Role.Create(
                    name,
                    description));
        }
    }

    private static async Task SeedRolePermissionsAsync(
        AuthDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var admin = await dbContext.Roles
            .SingleAsync(
                x => x.Name == "Admin",
                cancellationToken);

        var user = await dbContext.Roles
            .SingleAsync(
                x => x.Name == "User",
                cancellationToken);

        var permissions = await dbContext.Permissions
            .ToDictionaryAsync(
                x => x.Code,
                cancellationToken);

        var adminPermissions = permissions.Values;

        foreach (var permission in adminPermissions)
        {
            var exists = await dbContext.RolePermissions
                .AnyAsync(
                    x =>
                        x.RoleId == admin.Id &&
                        x.PermissionId == permission.Id,
                    cancellationToken);

            if (exists)
                continue;

            dbContext.RolePermissions.Add(
                RolePermission.Create(
                    admin.Id,
                    permission.Id));
        }

        var userPermissionCodes = new[]
        {
            "catalog.read",
            "product.read",
            Permissions.StoryRead,
            "discovery.read",
            "visit.read",
            Permissions.OrderingWrite
        };

        foreach (var code in userPermissionCodes)
        {
            var permission = permissions[code];

            var exists = await dbContext.RolePermissions
                .AnyAsync(
                    x =>
                        x.RoleId == user.Id &&
                        x.PermissionId == permission.Id,
                    cancellationToken);

            if (exists)
                continue;

            dbContext.RolePermissions.Add(
                RolePermission.Create(
                    user.Id,
                    permission.Id));
        }
    }
}