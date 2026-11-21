using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class OverdueInstallmentsModule
{
    public static IServiceCollection AddOverdueInstallments(
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

        // Reuse the Agreements DbContext so the worker reads the same
        // Installments / Agreements tables as the API.
        services.AddDbContext<AgreementsDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Reuse the Users DbContext to resolve debtor contact details
        // (phone / email) for notifications.
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Provider-neutral notification senders (SMS + email).
        services.AddNotifications(configuration);

        services.AddScoped<IOverdueInstallmentService, OverdueInstallmentService>();

        return services;
    }
}
