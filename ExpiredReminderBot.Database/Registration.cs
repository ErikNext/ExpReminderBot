using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpiredReminderBot.Database;

public static class Registration
{
    public static IServiceCollection InitializeContext(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<ExpiredReminderDbContext>(
            (s, b) =>
                b.UseNpgsql("configuration.GetConnectionString(\"ExpiredReminderDb\")"), ServiceLifetime.Singleton);

        return services;
    }
}