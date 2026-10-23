using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class NotificationsModule
{
    public static IServiceCollection AddNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        // The external SMS provider client.
        services.AddScoped<ISmsSender, TermiiSmsSender>();
        services.AddHttpClient<IEmailSender, BrevoEmailSender>();

        return services;
    }
}
