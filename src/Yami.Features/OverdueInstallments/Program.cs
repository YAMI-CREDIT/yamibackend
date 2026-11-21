using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// One-shot worker: runs the overdue-installment job once and exits.
// Scheduling is handled externally (a Kubernetes CronJob launches this
// container on a schedule). The process exits 0 on success and non-zero
// on failure so the orchestrator can detect a failed run.
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOverdueInstallments(builder.Configuration);

builder.Services.AddLogging(logging =>
    logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    }));

var host = builder.Build();
var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

await host.StartAsync();

try
{
    using var scope = host.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<IOverdueInstallmentService>();
    await service.ProcessOverdueInstallments(applicationLifetime.ApplicationStopping);
}
catch (OperationCanceledException)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Overdue installments job canceled by an admin.");
    await host.StopAsync();
    return 0;
}
catch (Exception exception)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "Overdue installments job failed.");
    await host.StopAsync();
    return 1;
}

await host.StopAsync();
return 0;
