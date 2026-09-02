using Microsoft.Extensions.Configuration;
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

    /// <summary>
    /// Cycle counter. Every FullSyncIntervalCycles iterations a full
    /// (non-incremental) sync is triggered to catch updates to existing rows.
    /// </summary>
    private int _cycleCount;

    /// <summary>
    /// How many sync cycles between full resyncs (e.g. 60 cycles × 10 s = 10 min).
    /// Read from appsettings.json; defaults to 60.
    /// </summary>
    private readonly int _fullSyncIntervalCycles;

    public SyncService(
        OpenEdgeRepository openEdgeRepo,
        MySqlRepository mySqlRepo,
        ILogger<SyncService> logger,
        IConfiguration configuration)
    {
        _openEdgeRepo = openEdgeRepo;
        _mySqlRepo = mySqlRepo;
        _logger = logger;
        _fullSyncIntervalCycles = configuration.GetValue<int>("Sync:FullSyncIntervalCycles", 60);
    }

    public async Task SyncIdPrintsAsync()
    {
        _cycleCount++;

        // Determine whether this is a full or incremental sync.
        int? maxBatchNo = null;                     // null = full sync
        bool isFullSync = true;

        if (_cycleCount > 1 && _cycleCount % _fullSyncIntervalCycles != 0)
        {
            // Incremental: only pull batches newer than what we already have.
            maxBatchNo = await _mySqlRepo.GetMaxBatchNoAsync();
            isFullSync = false;
        }

        string mode = isFullSync ? "FULL" : $"INCREMENTAL (batch_no > {maxBatchNo})";
        _logger.LogInformation("Starting id-print sync [{Mode}]: OpenEdge → MySQL...", mode);

        int synced = 0;
        int failed = 0;

        try
        {
            List<IdPrint> records = await RetryAsync(
                () => _openEdgeRepo.GetIdPrintsNewerThanAsync(!isFullSync ? maxBatchNo : null),
                $"Read id-print from OpenEdge ({mode})");

            if (records.Count == 0)
            {
                _logger.LogInformation("id-print sync complete [{Mode}]. No new records found.", mode);
                return;
            }

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
                "id-print sync complete [{Mode}]. {Synced} synced, {Failed} failed (out of {Total}).",
                mode, synced, failed, records.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "id-print sync failed [{Mode}].", mode);
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
