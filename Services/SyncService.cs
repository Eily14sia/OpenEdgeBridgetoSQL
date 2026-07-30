using Microsoft.Extensions.Logging;
using OpenEdgeBridge.Models;
using OpenEdgeBridge.Repositories;

namespace OpenEdgeBridge.Services;

public class SyncService
{
    private readonly OpenEdgeRepository _openEdgeRepo;
    private readonly MySqlRepository _mySqlRepo;
    private readonly ILogger<SyncService> _logger;
    private const int MaxRetries = 3;

    public SyncService(
        OpenEdgeRepository openEdgeRepo,
        MySqlRepository mySqlRepo,
        ILogger<SyncService> logger)
    {
        _openEdgeRepo = openEdgeRepo;
        _mySqlRepo = mySqlRepo;
        _logger = logger;
    }

    public async Task SyncIdPrintsAsync()
    {
        _logger.LogInformation("Starting id-print sync: OpenEdge → MySQL...");

        int synced = 0;
        int failed = 0;

        try
        {
            List<IdPrint> records = await RetryAsync(
                () => _openEdgeRepo.GetIdPrintsAsync(),
                "Read id-print from OpenEdge");

            foreach (var record in records)
            {
                try
                {
                    await RetryAsync(
                        () => _mySqlRepo.UpsertIdPrintAsync(record),
                        $"Upsert id-print batch={record.BatchNo} app={record.AppNo}");
                    synced++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to upsert id-print batch={BatchNo} app={AppNo}",
                        record.BatchNo, record.AppNo);
                    failed++;
                }
            }

            _logger.LogInformation(
                "id-print sync complete. {Synced} synced, {Failed} failed (out of {Total}).",
                synced, failed, records.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "id-print sync failed.");
        }
    }

    public async Task SyncPrintedBackToOpenEdgeAsync()
    {
        _logger.LogInformation("Starting print_count back-sync: MySQL → OpenEdge...");

        int updated = 0;
        int failed = 0;

        try
        {
            List<int> appNos = await RetryAsync(
                () => _mySqlRepo.GetPrintedAppNosAsync(),
                "Read printed app_nos from MySQL");

            foreach (int appNo in appNos)
            {
                try
                {
                    // Mark both sides as printed
                    await RetryAsync(
                        () => _openEdgeRepo.MarkPrintedAsync(appNo),
                        $"Mark app-no={appNo} as printed in OpenEdge");
                    await RetryAsync(
                        () => _mySqlRepo.MarkPrintedAsync(appNo),
                        $"Mark app-no={appNo} as printed in MySQL");
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to mark app-no={AppNo} as printed", appNo);
                    failed++;
                }
            }

            _logger.LogInformation(
                "Print count back-sync complete. {Updated} marked printed, {Failed} failed (out of {Total}).",
                updated, failed, appNos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Print count back-sync failed.");
        }
    }

    private static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        string description,
        int maxRetries = MaxRetries)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < maxRetries)
            {
                int delayMs = attempt * 1000;
                await Task.Delay(delayMs);
            }
        }

        return await operation();
    }

    private static async Task RetryAsync(
        Func<Task> operation,
        string description,
        int maxRetries = MaxRetries)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception) when (attempt < maxRetries)
            {
                int delayMs = attempt * 1000;
                await Task.Delay(delayMs);
            }
        }

        await operation();
    }
}
