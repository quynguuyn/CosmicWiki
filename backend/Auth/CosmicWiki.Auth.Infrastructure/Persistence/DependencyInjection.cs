using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Infrastructure.Persistence;
using CosmicWiki.Auth.Infrastructure.Persistence.Repositories;
using CosmicWiki.Auth.Infrastructure.Security;
using CosmicWiki.Auth.Persistence.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Auth.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseOracle(
                configuration.GetConnectionString("AuthDatabase"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AuthDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        services.AddSingleton<ITokenService, JwtTokenService>();
        
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}