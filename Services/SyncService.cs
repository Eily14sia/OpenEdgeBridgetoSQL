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

    /// <summary>
    /// Which client's rows this deployment syncs (Sync:Client in appsettings.json
    /// — "Both" | "RTC" | "OSCA"; overridable with the Sync__Client environment
    /// variable). Restricts the OpenEdge read, the MySQL watermark, the upsert,
    /// and the printed back-sync to a single ID kind.
    /// </summary>
    private readonly IdPrintKind _clientKind;

    /// <summary>
    /// Set once the MySQL `id_print` table has been confirmed to contain every
    /// column required for <see cref="_clientKind"/>. Avoids re-querying the
    /// schema on every cycle.
    /// </summary>
    private bool _schemaVerified;

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
        _clientKind = IdPrintKindExtensions.ParseClient(configuration.GetValue<string>("Sync:Client"));
    }

    public async Task SyncIdPrintsAsync()
    {
        // Fail fast with a clear message if the MySQL table doesn't match the
        // configured client (see Sync:Client). Checked once per process.
        if (!await VerifySchemaOnceAsync())
            return;

        // Determine whether this is a full or incremental sync.
        // FULL runs when the persisted last-full-sync timestamp is missing
        // or older than Sync:FullSyncIntervalMinutes. Restarting the service
        // does NOT by itself force a full sync.
        long? maxTransNo = null;                    // null = full sync
        bool isFullSync = true;

        DateTime? lastFullSyncUtc = null;
        try
        {
            lastFullSyncUtc = await _mySqlRepo.GetLastFullSyncUtcAsync(_clientKind);
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
            // Incremental: only pull rows newer than what we already have.
            // Watermark is trans_no (the universal PRIMARY key on every row),
            // because batch_no is NULL on OSCA/senior rows.
            maxTransNo = await _mySqlRepo.GetMaxTransNoAsync(_clientKind);
            isFullSync = false;
        }

        string mode = isFullSync ? "FULL" : $"INCREMENTAL (trans_no > {maxTransNo})";
        _logger.LogInformation(
            "Starting id-print sync [{Mode}] for client {Client}: OpenEdge → MySQL...",
            mode, _clientKind.Describe());

        int synced = 0;
        int failed = 0;
        int skipped = 0;

        try
        {
            List<IdPrint> records = await RetryAsync(
                () => _openEdgeRepo.GetIdPrintsNewerThanAsync(!isFullSync ? maxTransNo : null, _clientKind),
                $"Read id-print from OpenEdge ({mode}, client {_clientKind.Describe()})");

            if (records.Count == 0)
            {
                _logger.LogInformation(
                    "id-print sync complete [{Mode}] for client {Client}. No new records found.",
                    mode, _clientKind.Describe());
                if (isFullSync)
                    await PersistLastFullSyncAsync();
                return;
            }

            int? expectedTranType = _clientKind.TranTypeFilter();

            foreach (var record in records)
            {
                // Defensive guard: never create a row for the other client, even
                // if one slipped past the OpenEdge WHERE filter (e.g. a legacy
                // row whose tran-type is missing). Only the configured client is
                // ever written to MySQL.
                if (expectedTranType.HasValue && record.TranType != expectedTranType.Value)
                {
                    _logger.LogWarning(
                        "Skipping trans_no={TransNo} (tran_type={TranType}) — client filter is {Client}.",
                        record.TransNo, record.TranType, _clientKind.Describe());
                    skipped++;
                    continue;
                }

                try
                {
                    await RetryAsync(
                        () => _mySqlRepo.UpsertIdPrintAsync(record, _clientKind),
                        $"Upsert id-print trans={record.TransNo} batch={record.BatchNo} app={record.AppNo}");
                    synced++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to upsert id-print trans_no={TransNo} batch_no={BatchNo} app_no={AppNo}",
                        record.TransNo, record.BatchNo, record.AppNo);
                    failed++;
                }
            }

            _logger.LogInformation(
                "id-print sync complete [{Mode}] for client {Client}. {Synced} synced, {Skipped} skipped, {Failed} failed (out of {Total}).",
                mode, _clientKind.Describe(), synced, skipped, failed, records.Count);

            if (isFullSync)
                await PersistLastFullSyncAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "id-print sync failed [{Mode}].", mode);
        }
    }

    /// <summary>
    /// Confirms (once per process) that the MySQL `id_print` table contains every
    /// column the bridge writes for the configured client. Returns false — and
    /// logs an actionable error — when the table and Sync:Client disagree, so the
    /// cycle is skipped instead of failing row-by-row with "unknown column".
    /// </summary>
    private async Task<bool> VerifySchemaOnceAsync()
    {
        if (_schemaVerified)
            return true;

        try
        {
            List<string> missing = await _mySqlRepo.GetMissingIdPrintColumnsAsync(_clientKind);
            if (missing.Count > 0)
            {
                _logger.LogError(
                    "MySQL table `id_print` is missing column(s) required for client {Client}: {Missing}. " +
                    "Recreate `id_print` to match Sync:Client (see the setup guide). Skipping this cycle.",
                    _clientKind.Describe(), string.Join(", ", missing));
                return false;
            }

            _schemaVerified = true;
            return true;
        }
        catch (Exception ex)
        {
            // MySQL may be temporarily unavailable — let the normal sync attempt
            // surface the real error rather than blocking here.
            _logger.LogWarning(ex, "Could not verify the id_print schema; continuing.");
            return true;
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
                () => _mySqlRepo.SetLastFullSyncUtcAsync(DateTime.UtcNow, _clientKind),
                "Persist last full-sync timestamp");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist last full-sync timestamp; next cycle will run FULL again.");
        }
    }

    public async Task SyncPrintedBackToOpenEdgeAsync()
    {
        _logger.LogInformation("Starting print_count back-sync for client {Client}: MySQL → OpenEdge...",
            _clientKind.Describe());

        int updated = 0;
        int failed = 0;

        try
        {
            List<PrintedIdPrint> pending = await RetryAsync(
                () => _mySqlRepo.GetPrintedBackSyncItemsAsync(_clientKind),
                "Read printed rows from MySQL");

            foreach (PrintedIdPrint item in pending)
            {
                try
                {
                    // Once print_count reaches 1 (the back-sync trigger), stamp the
                    // printed date/time with THIS machine's local time in both stores.
                    // Instant ID's own printed_dt/printed_tm are ignored. photo/
                    // image-data are carried back when present; pvc_no never.
                    DateTime printedAt = DateTime.Now;
                    await RetryAsync(
                        () => _openEdgeRepo.MarkPrintedAsync(
                            item.TransNo, printedAt, item.Photo, item.ImageData),
                        $"Mark trans-no={item.TransNo} as printed in OpenEdge");
                    await RetryAsync(
                        () => _mySqlRepo.MarkPrintedAsync(item.TransNo, printedAt),
                        $"Mark trans-no={item.TransNo} as printed in MySQL");
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to mark trans-no={TransNo} as printed", item.TransNo);
                    failed++;
                }
            }

            _logger.LogInformation(
                "Print count back-sync complete. {Updated} marked printed, {Failed} failed (out of {Total}).",
                updated, failed, pending.Count);
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
