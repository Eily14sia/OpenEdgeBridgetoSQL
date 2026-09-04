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
    /// How often a full (non-incremental) sync may run, measured from the last
    /// successful full sync persisted in MySQL. Set to 0 to DISABLE periodic
    /// full syncs entirely — only the initial seed (first run, no stored
    /// timestamp) performs a full sync. Use 0 when OpenEdge id-print rows are
    /// write-once (new batches only, no in-place edits).
    /// Read from appsettings.json (Sync:FullSyncIntervalMinutes); defaults to 10.
    /// </summary>
    private readonly int _fullSyncIntervalMinutes;

    public SyncService(
        OpenEdgeRepository openEdgeRepo,
        MySqlRepository mySqlRepo,
        ILogger<SyncService> logger,
        IConfiguration configuration)
    {
        _openEdgeRepo = openEdgeRepo;
        _mySqlRepo = mySqlRepo;
        _logger = logger;
        _fullSyncIntervalMinutes = configuration.GetValue<int>("Sync:FullSyncIntervalMinutes", 10);
    }

    public async Task SyncIdPrintsAsync()
    {
        // Determine whether this is a full or incremental sync.
        // FULL runs when the persisted last-full-sync timestamp is missing
        // or older than Sync:FullSyncIntervalMinutes. Restarting the service
        // does NOT by itself force a full sync.
        int? maxBatchNo = null;                     // null = full sync
        bool isFullSync = true;

        DateTime? lastFullSyncUtc = null;
        try
        {
            lastFullSyncUtc = await _mySqlRepo.GetLastFullSyncUtcAsync();
        }
        catch (Exception ex)
        {
            // If state can't be read (e.g. MySQL briefly down), a full sync
            // is the safe default — it will repopulate everything.
            _logger.LogWarning(ex,
                "Could not read last full-sync state from MySQL; falling back to FULL sync.");
        }

        if (lastFullSyncUtc.HasValue &&
            (_fullSyncIntervalMinutes <= 0 ||
             DateTime.UtcNow - lastFullSyncUtc.Value < TimeSpan.FromMinutes(_fullSyncIntervalMinutes)))
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
                if (isFullSync)
                    await PersistLastFullSyncAsync();
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

            if (isFullSync)
                await PersistLastFullSyncAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "id-print sync failed [{Mode}].", mode);
        }
    }

    /// <summary>
    /// Records the completion time of a FULL sync in MySQL so that restarts
    /// (or a failed state read) do not force another full sync before the
    /// configured interval elapses. If persisting fails, the next cycle will
    /// simply run FULL again — the safe default.
    /// </summary>
    private async Task PersistLastFullSyncAsync()
    {
        try
        {
            await RetryAsync(
                () => _mySqlRepo.SetLastFullSyncUtcAsync(DateTime.UtcNow),
                "Persist last full-sync timestamp");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist last full-sync timestamp; next cycle will run FULL again.");
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
