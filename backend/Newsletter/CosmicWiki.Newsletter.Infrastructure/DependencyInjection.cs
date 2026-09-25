using CosmicWiki.Newsletter.Application;
using CosmicWiki.Newsletter.Application.Abstractions;
using CosmicWiki.Newsletter.Infrastructure.Delivery;
using CosmicWiki.Newsletter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Newsletter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNewsletterInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NewsletterDbContext>(options =>
            options.UseOracle(
                configuration.GetConnectionString("NewsletterDatabase")));

        services.Configure<NewsletterOptions>(
            configuration.GetSection("Newsletter"));
        services.AddSingleton(sp =>
        {
            var options = new NewsletterOptions();
            configuration.GetSection("Newsletter").Bind(options);
            return options;
        });
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<INewsletterSubscriptionRepository, NewsletterSubscriptionRepository>();
        services.AddScoped<INewsletterDeliveryRepository, NewsletterDeliveryRepository>();
        services.AddScoped<INewsletterUnitOfWork>(sp =>
            sp.GetRequiredService<NewsletterDbContext>());
        services.AddSingleton<IConfirmationTokenService, ConfirmationTokenService>();
        services.AddSingleton<IEmailSender, DevEmailSender>();
        services.AddHostedService<NewsletterDeliveryProcessor>();

        return services;
    }
}
