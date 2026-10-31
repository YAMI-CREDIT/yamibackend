using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

public static class NotificationsModule
{
    public static IServiceCollection AddNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddResend(options =>
            options.ApiToken = configuration[$"{EmailOptions.SectionName}:ApiKey"] ?? string.Empty);

        // The external SMS provider client.
        services.AddScoped<ISmsSender, TermiiSmsSender>();
        services.AddScoped<IEmailSender, ResendEmailSender>();
        // services.AddHttpClient<IEmailSender, BrevoEmailSender>();

        return services;
    }
}
