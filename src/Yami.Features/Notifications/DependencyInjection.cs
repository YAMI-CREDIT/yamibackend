using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class NotificationsModule
{
    public static IServiceCollection AddNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));

        // The external SMS provider client.
        services.AddScoped<ISmsSender, TermiiSmsSender>();

        return services;
    }
}
