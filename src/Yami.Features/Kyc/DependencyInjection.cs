using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class KycModule
{
    public static IServiceCollection AddKyc(
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

        services.AddDbContext<KycDbContext>(options =>
            options.UseNpgsql(connectionString));

        // The external provider client.
        services.AddScoped<IKycProvider, KycProvider>();

        services.AddScoped<IKycService, KycService>();

        return services;
    }
}
