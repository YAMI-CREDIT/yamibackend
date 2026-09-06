using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class UsersModule
{
    public static IServiceCollection AddUsers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is missing.");
        }

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        // DEV STUBS - swap these for real providers (Twilio/Termii/SNS, SendGrid/SES/SMTP) before production.
        services.AddScoped<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        services.AddControllers(options =>
            {
                options.Filters.Add<AuthExceptionFilter>();
            })
            .AddApplicationPart(typeof(AuthController).Assembly);

        return services;
    }
}