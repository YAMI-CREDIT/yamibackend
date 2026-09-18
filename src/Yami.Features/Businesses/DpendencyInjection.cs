using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
public static class BusinessesModule
{
    public static IServiceCollection AddBusinesses(
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

        services.AddDbContext<BusinessesDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBusinessService, BusinessService>();

        // services.AddControllers()
        //     .AddApplicationPart(typeof(RegisterBusinessController).Assembly);

        return services;
    }
}