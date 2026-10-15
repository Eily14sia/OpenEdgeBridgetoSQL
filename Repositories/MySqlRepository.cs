using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OpenEdgeBridge.Models;

namespace OpenEdgeBridge.Repositories;

public class MySqlRepository : IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<MySqlRepository> _logger;

    // OpenEdge "id-print" holds two ID kinds distinguished by tran_type; the
    // natural key used for the MySQL upsert differs per kind:
    //   1 = RTC          -> key is (batch_no, app_no)     (these are NOT null)
    //   2 = OSCA/senior  -> key is (control_no, id_no)    (batch_no/app_no are NULL)
    private const int TranTypeRtc = 1;
    private const int TranTypeOsca = 2;

    public MySqlRepository(IConfiguration configuration, ILogger<MySqlRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySQL connection string not found.");
        _logger = logger;
    }

    // --- Columns the bridge writes, grouped per client -------------------------
    // These mirror OpenEdgeRepository's projection, so a single-client deployment
    // only needs that client's columns in its `id_print` table. The Entrust-only
    // image columns (signature_data, qr_data, fingerprint_data) are NOT written
    // here, so the sync never overwrites them.
    private static readonly string[] CommonDataColumns =
    {
        "trans_no", "trans_date", "trans_time",
        "address", "cp_no", "dob",
        "lname", "fname", "mname", "sfx", "fullname",
        "gender", "id_type", "image_data", "photo",
        "printed", "printed_dt", "printed_tm", "tran_type", "pvc_no"
    };

    private static readonly string[] RtcOnlyDataColumns =
    {
        "batch_no", "batch_date", "app_no", "book_no", "page_no",
        "series_no", "spa_docno", "spa_atty", "spa_dt_issue",
        "id_code", "hcp_cpno", "hcp_fullname"
    };

    private static readonly string[] OscaOnlyDataColumns =
    {
        "age", "control_no", "date_issued", "id_no", "year_applied"
    };

    /// <summary>
    /// The MySQL columns the bridge writes for a given client (excludes the
    /// Entrust-only image columns). Used to build the upsert and to validate a
    /// per-client table up front.
    /// </summary>
    public static IReadOnlyList<string> WrittenColumnsFor(IdPrintKind kind)
    {
        var columns = new List<string>(CommonDataColumns);
        if (kind != IdPrintKind.Osca)
            columns.AddRange(RtcOnlyDataColumns);
        if (kind != IdPrintKind.Rtc)
            columns.AddRange(OscaOnlyDataColumns);
        columns.Add("print_count");
        return columns;
    }

    private static List<(string Column, object? Value)> BuildValues(IdPrint r, IdPrintKind kind)
    {
        var values = new List<(string, object?)>
        {
            ("trans_no", r.TransNo), ("trans_date", r.TransDate), ("trans_time", r.TransTime),
            ("address", r.Address), ("cp_no", r.CpNo), ("dob", r.Dob),
            ("lname", r.Lname), ("fname", r.Fname), ("mname", r.Mname),
            ("sfx", r.Sfx), ("fullname", r.Fullname),
            ("gender", r.Gender), ("id_type", r.IdType),
            ("image_data", r.ImageData), ("photo", r.Photo),
            ("printed", r.Printed), ("printed_dt", r.PrintedDt), ("printed_tm", r.PrintedTm),
            ("tran_type", r.TranType), ("pvc_no", r.PvcNo)
        };

        if (kind != IdPrintKind.Osca)
        {
            values.Add(("batch_no", r.BatchNo));
            values.Add(("batch_date", r.BatchDate));
            values.Add(("app_no", r.AppNo));
            values.Add(("book_no", r.BookNo));
            values.Add(("page_no", r.PageNo));
            values.Add(("series_no", r.SeriesNo));
            values.Add(("spa_docno", r.SpaDocno));
            values.Add(("spa_atty", r.SpaAtty));
            values.Add(("spa_dt_issue", r.SpaDtIssue));
            values.Add(("id_code", r.IdCode));
            values.Add(("hcp_cpno", r.HcpCpno));
            values.Add(("hcp_fullname", r.HcpFullname));
        }

        if (kind != IdPrintKind.Rtc)
        {
            values.Add(("age", r.Age));
            values.Add(("control_no", r.ControlNo));
            values.Add(("date_issued", r.DateIssued));
            values.Add(("id_no", r.IdNo));
            values.Add(("year_applied", r.YearApplied));
        }

        return values;
    }

    public async Task UpsertIdPrintAsync(IdPrint record, IdPrintKind kind = IdPrintKind.Both)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // OpenEdge "id-print" holds two ID kinds in one table, distinguished by
        // tran_type. Every row (both kinds) now carries a table-wide unique
        // "trans-no" (PRIMARY index id-idx in the updated .df), so it is the
        // universal MySQL key:
        //   tran_type 1 (RTC)          -> business key = batch_no + app_no
        //   tran_type 2 (OSCA/senior)  -> business key = control_no + id_no
        // Use trans_no when it is actually populated (> 0); fall back to the
        // type-specific key only for legacy rows whose trans_no is still 0.
        bool isOsca = record.TranType == TranTypeOsca
            // Fall back for legacy rows whose tran_type is missing/unknown but
            // that clearly aren't RTC rows (no batch_no AND no app_no).
            || (record.TranType != TranTypeRtc && !record.BatchNo.HasValue && !record.AppNo.HasValue);

        bool useTransNo = record.TransNo.HasValue && record.TransNo.Value > 0;

        string whereClause;
        if (useTransNo)
        {
            whereClause = "trans_no = @trans_no";
        }
        else if (isOsca)
        {
            whereClause = "control_no = @control_no AND id_no = @id_no";
        }
        else
        {
            whereClause = "batch_no = @batch_no AND app_no = @app_no";
        }

        if (!useTransNo && isOsca && (!record.ControlNo.HasValue || string.IsNullOrEmpty(record.IdNo)))
            _logger.LogWarning(
                "OSCA record has no usable key (trans_no={TransNo}, control_no={ControlNo}, id_no={IdNo}); sync will not dedupe it.",
                record.TransNo, record.ControlNo, record.IdNo);

        // Check if the record already exists by its key to avoid burning
        // auto-increment IDs on every INSERT ... ON DUPLICATE KEY UPDATE cycle.
        // (InnoDB allocates the next auto-increment value even when the INSERT
        //  becomes an UPDATE, causing large gaps over repeated sync runs.)
        List<(string Column, object? Value)> values = BuildValues(record, kind);

        string checkSql = $"SELECT COUNT(1) FROM id_print WHERE {whereClause}";
        await using var checkCmd = new MySqlCommand(checkSql, connection);
        AddKeyParameters(checkCmd, record, useTransNo, isOsca);
        bool exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;

        if (exists)
        {
            string setList = string.Join(",\n        ", values.Select(v => $"{v.Column} = @{v.Column}"));
            string updateSql = $"UPDATE id_print\n        SET\n        {setList}\n        WHERE {whereClause}";

            await using var cmd = new MySqlCommand(updateSql, connection);
            AddValueParameters(cmd, values);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Updated id-print record trans_no={TransNo} type={TranType} batch_no={BatchNo} app_no={AppNo}",
                record.TransNo, record.TranType, record.BatchNo, record.AppNo);
        }
        else
        {
            string columnList = string.Join(",\n        ", values.Select(v => v.Column).Append("print_count"));
            string valueList  = string.Join(",\n        ", values.Select(v => "@" + v.Column).Append("@print_count"));
            string insertSql = $"INSERT INTO id_print\n        (\n        {columnList}\n        )\n        VALUES\n        (\n        {valueList}\n        )";

            await using var cmd = new MySqlCommand(insertSql, connection);
            AddValueParameters(cmd, values);
            cmd.Parameters.AddWithValue("@print_count", (object?)record.PrintCount ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Inserted id-print record trans_no={TransNo} type={TranType} batch_no={BatchNo} app_no={AppNo}",
                record.TransNo, record.TranType, record.BatchNo, record.AppNo);
        }
    }

    private static void AddValueParameters(MySqlCommand cmd, List<(string Column, object? Value)> values)
    {
        foreach (var (column, value) in values)
            cmd.Parameters.AddWithValue("@" + column, value ?? DBNull.Value);
    }

    private static void AddKeyParameters(MySqlCommand cmd, IdPrint record, bool useTransNo, bool isOsca)
    {
        if (useTransNo)
        {
            cmd.Parameters.AddWithValue("@trans_no", (object?)record.TransNo ?? DBNull.Value);
        }
        else if (isOsca)
        {
            cmd.Parameters.AddWithValue("@control_no", (object?)record.ControlNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id_no", (object?)record.IdNo ?? DBNull.Value);
        }
        else
        {
            cmd.Parameters.AddWithValue("@batch_no", (object?)record.BatchNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@app_no", (object?)record.AppNo ?? DBNull.Value);
        }
    }

    /// <summary>
    /// Returns the columns the bridge writes for <paramref name="kind"/> that do
    /// not exist in the current `id_print` table (an empty list means the schema
    /// is fine). Lets the service warn clearly when Sync:Client and the table
    /// layout disagree.
    /// </summary>
    public async Task<List<string>> GetMissingIdPrintColumnsAsync(IdPrintKind kind)
    {
        IReadOnlyList<string> expected = WrittenColumnsFor(kind);

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'id_print'
            """;

        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var cmd = new MySqlCommand(sql, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                existing.Add(reader.GetString(0));
        }

        return expected.Where(c => !existing.Contains(c)).ToList();
    }

    private const string LastFullSyncKey = "last_full_sync_utc";

    /// <summary>
    /// The sync_state key that records the last FULL sync timestamp. The key is
    /// per-client, so switching Sync:Client on an existing deployment seeds a
    /// fresh FULL sync for the newly selected client (the "Both" key is kept
    /// unchanged for backwards compatibility).
    /// </summary>
    private static string FullSyncKeyFor(IdPrintKind kind) => kind switch
    {
        IdPrintKind.Rtc  => "last_full_sync_utc_rtc",
        IdPrintKind.Osca => "last_full_sync_utc_osca",
        _                => LastFullSyncKey
    };

    /// <summary>
    /// Creates the sync_state table if it does not exist yet.
    /// Called lazily before every state read/write so the bridge
    /// self-heals even if MySQL was unavailable at startup.
    /// </summary>
    private static async Task EnsureSyncStateTableAsync(MySqlConnection connection)
    {
        const string createSql = """
            CREATE TABLE IF NOT EXISTS sync_state (
                state_key   VARCHAR(64)  NOT NULL PRIMARY KEY,
                state_value VARCHAR(64)  NOT NULL,
                updated_utc DATETIME     NOT NULL
            ) ENGINE=InnoDB
            """;

        await using var cmd = new MySqlCommand(createSql, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Reads the UTC timestamp of the last successful FULL sync for the given
    /// client, or null if a full sync has never been recorded for it.
    /// </summary>
    public async Task<DateTime?> GetLastFullSyncUtcAsync(IdPrintKind kind = IdPrintKind.Both)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await EnsureSyncStateTableAsync(connection);

        const string sql = "SELECT state_value FROM sync_state WHERE state_key = @key";
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@key", FullSyncKeyFor(kind));

        object? result = await cmd.ExecuteScalarAsync();
        if (result == null || result is DBNull)
            return null;

        string value = Convert.ToString(result)!;
        return DateTime.TryParse(value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal
                | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out DateTime parsed)
            ? parsed
            : null;
    }

    /// <summary>
    /// Persists the UTC timestamp of a successful FULL sync for the given client.
    /// </summary>
    public async Task SetLastFullSyncUtcAsync(DateTime utc, IdPrintKind kind = IdPrintKind.Both)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await EnsureSyncStateTableAsync(connection);

        const string sql = """
            INSERT INTO sync_state (state_key, state_value, updated_utc)
            VALUES (@key, @value, UTC_TIMESTAMP())
            ON DUPLICATE KEY UPDATE
                state_value = @value,
                updated_utc = UTC_TIMESTAMP()
            """;

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@key", FullSyncKeyFor(kind));
        cmd.Parameters.AddWithValue("@value",
            utc.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Returns the highest trans_no currently stored in MySQL.
    /// Used for incremental sync — only fetch rows whose OpenEdge trans-no is
    /// greater than this. trans-no is the table-wide PRIMARY key (id-idx) and
    /// exists on every row (RTC and OSCA/senior alike), so it is a safe
    /// universal watermark unlike batch_no (which is NULL for OSCA rows).
    /// When a single client is configured, the watermark is scoped to that
    /// client (tran_type) so a higher trans_no of the *other* kind can never
    /// hide un-synced rows of the configured kind.
    /// </summary>
    public async Task<long?> GetMaxTransNoAsync(IdPrintKind kind = IdPrintKind.Both)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        int? tranTypeFilter = kind.TranTypeFilter();

        string sql = tranTypeFilter.HasValue
            ? "SELECT MAX(trans_no) FROM id_print WHERE tran_type = @tran_type"
            : "SELECT MAX(trans_no) FROM id_print";
        await using var cmd = new MySqlCommand(sql, connection);
        if (tranTypeFilter.HasValue)
            cmd.Parameters.AddWithValue("@tran_type", tranTypeFilter.Value);
        object? result = await cmd.ExecuteScalarAsync();

        if (result == null || result is DBNull)
            return null;

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Returns the rows that Instant ID has printed (print_count > 0) but that
    /// are not yet marked as printed. Alongside trans_no it reads the image fields
    /// Entrust may have filled while printing (photo, image_data) so the back-sync
    /// can carry them to OpenEdge. The printed date/time are NOT read — the
    /// back-sync stamps them with the bridge machine's local time instead.
    /// NOTE: pvc_no is deliberately NOT read back — it always holds a value in
    /// MySQL, so pushing it would overwrite OpenEdge's PVC-No on every sync.
    /// Scoped to the configured client (tran_type) so an OSCA-only deployment
    /// never pushes prints back onto RTC rows (and vice versa).
    /// </summary>
    public async Task<List<PrintedIdPrint>> GetPrintedBackSyncItemsAsync(IdPrintKind kind = IdPrintKind.Both)
    {
        var items = new List<PrintedIdPrint>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        int? tranTypeFilter = kind.TranTypeFilter();

        string sql = tranTypeFilter.HasValue
            ? """
                SELECT trans_no, photo, image_data
                FROM id_print
                WHERE trans_no > 0
                  AND print_count > 0
                  AND (printed IS NULL OR printed = 'no')
                  AND tran_type = @tran_type
                """
            : """
                SELECT trans_no, photo, image_data
                FROM id_print
                WHERE trans_no > 0
                  AND print_count > 0
                  AND (printed IS NULL OR printed = 'no')
                """;

        await using var cmd = new MySqlCommand(sql, connection);
        if (tranTypeFilter.HasValue)
            cmd.Parameters.AddWithValue("@tran_type", tranTypeFilter.Value);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (reader.IsDBNull(0))
                continue;

            items.Add(new PrintedIdPrint
            {
                TransNo   = reader.GetInt64(0),
                Photo     = reader.IsDBNull(1) ? null : (byte[])reader[1],
                ImageData = reader.IsDBNull(2) ? null : (byte[])reader[2]
            });
        }

        return items;
    }

    /// <summary>
    /// Marks a row as printed and stamps <c>printed_dt</c>/<c>printed_tm</c> with
    /// the bridge machine's local time (<paramref name="printedAt"/>) rather than
    /// whatever Instant ID wrote, so MySQL and OpenEdge agree. Called once
    /// <c>print_count</c> reaches 1 — the back-sync trigger.
    /// </summary>
    public async Task MarkPrintedAsync(long transNo, DateTime printedAt)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE id_print
            SET printed = 'yes',
                printed_dt = @printed_dt,
                printed_tm = @printed_tm
            WHERE trans_no = @trans_no
            """;

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@trans_no", transNo);
        cmd.Parameters.AddWithValue("@printed_dt", printedAt.Date);
        cmd.Parameters.AddWithValue("@printed_tm",
            printedAt.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        // MySqlConnection is disposed via `await using` in each method.
    }
}
