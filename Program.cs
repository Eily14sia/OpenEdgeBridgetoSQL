using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenEdgeBridge.Repositories;
using OpenEdgeBridge.Services;

namespace OpenEdgeBridge;

public class Program
{
    public static async Task Main(string[] args)
    {
        // -----------------------------------------------------------------
        // Build the generic host with DI, configuration, and console logging.
        // When ready for Phase 5, change UseConsoleLifetime() to
        // UseWindowsService() for production deployment.
        // -----------------------------------------------------------------
        using IHost host = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "OpenEdgeBridge";
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<OpenEdgeRepository>();
                services.AddSingleton<MySqlRepository>();
                services.AddSingleton<SyncService>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();

                if (OperatingSystem.IsWindows())
                {
#pragma warning disable CA1416 // EventLog is Windows-only — guarded by IsWindows()
                    logging.AddEventLog(settings =>
                    {
                        settings.SourceName = "OpenEdgeBridge";
                    });
#pragma warning restore CA1416
                }

                // Also log to a file in the Logs folder
                string logPath = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logPath);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var syncService = host.Services.GetRequiredService<SyncService>();

        int intervalSeconds = configuration.GetValue<int>("Sync:IntervalSeconds", 10);
        logger.LogInformation("OpenEdge Bridge started. Sync interval: {Interval}s", intervalSeconds);
        logger.LogInformation("Press Ctrl+C to stop.");

        // -----------------------------------------------------------------
        // Main sync loop — runs until the process is shut down.
        // -----------------------------------------------------------------
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            logger.LogInformation("Shutdown signal received. Stopping...");
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                // Phase 3: OpenEdge → MySQL
                await syncService.SyncOpenEdgeToMySqlAsync();

                // Phase 4: MySQL print_count → OpenEdge
                await syncService.SyncPrintCountBackToOpenEdgeAsync();

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Fatal error in sync loop.");
        }

        logger.LogInformation("OpenEdge Bridge stopped.");
    }
}
