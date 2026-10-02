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

    public async Task UpsertIdPrintAsync(IdPrint record)
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
        string checkSql = $"SELECT COUNT(1) FROM id_print WHERE {whereClause}";
        await using var checkCmd = new MySqlCommand(checkSql, connection);
        AddUpsertParameters(checkCmd, record);
        bool exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;

        if (exists)
        {
            string updateSql = $$"""
                UPDATE id_print
                SET
                    trans_no      = @trans_no,
                    trans_date    = @trans_date,
                    trans_time    = @trans_time,
                    batch_no      = @batch_no,
                    batch_date    = @batch_date,
                    app_no        = @app_no,
                    dob           = @dob,
                    cp_no         = @cp_no,
                    fullname      = @fullname,
                    hcp_fullname  = @hcp_fullname,
                    address       = @address,
                    hcp_cpno      = @hcp_cpno,
                    lname         = @lname,
                    fname         = @fname,
                    mname         = @mname,
                    spa_docno     = @spa_docno,
                    page_no       = @page_no,
                    book_no       = @book_no,
                    series_no     = @series_no,
                    spa_atty      = @spa_atty,
                    spa_dt_issue  = @spa_dt_issue,
                    printed       = @printed,
                    printed_dt    = @printed_dt,
                    printed_tm    = @printed_tm,
                    tran_type     = @tran_type,
                    image_data    = @image_data,
                    id_code       = @id_code,
                    sfx           = @sfx,
                    photo         = @photo,
                    gender        = @gender,
                    date_issued   = @date_issued,
                    id_no         = @id_no,
                    year_applied  = @year_applied,
                    control_no    = @control_no,
                    id_type       = @id_type,
                    pvc_no        = @pvc_no,
                    age           = @age
                WHERE {{whereClause}}
                """;

            await using var cmd = new MySqlCommand(updateSql, connection);
            AddUpsertParameters(cmd, record);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Updated id-print record trans_no={TransNo} type={TranType} batch_no={BatchNo} app_no={AppNo}",
                record.TransNo, record.TranType, record.BatchNo, record.AppNo);
        }
        else
        {
            const string insertSql = """
                INSERT INTO id_print
                (
                    trans_no,
                    trans_date,
                    trans_time,
                    batch_no,
                    batch_date,
                    app_no,
                    dob,
                    cp_no,
                    fullname,
                    hcp_fullname,
                    address,
                    hcp_cpno,
                    lname,
                    fname,
                    mname,
                    spa_docno,
                    page_no,
                    book_no,
                    series_no,
                    spa_atty,
                    spa_dt_issue,
                    printed,
                    printed_dt,
                    printed_tm,
                    tran_type,
                    image_data,
                    id_code,
                    sfx,
                    photo,
                    gender,
                    date_issued,
                    id_no,
                    year_applied,
                    control_no,
                    id_type,
                    pvc_no,
                    age,
                    print_count
                )
                VALUES
                (
                    @trans_no,
                    @trans_date,
                    @trans_time,
                    @batch_no,
                    @batch_date,
                    @app_no,
                    @dob,
                    @cp_no,
                    @fullname,
                    @hcp_fullname,
                    @address,
                    @hcp_cpno,
                    @lname,
                    @fname,
                    @mname,
                    @spa_docno,
                    @page_no,
                    @book_no,
                    @series_no,
                    @spa_atty,
                    @spa_dt_issue,
                    @printed,
                    @printed_dt,
                    @printed_tm,
                    @tran_type,
                    @image_data,
                    @id_code,
                    @sfx,
                    @photo,
                    @gender,
                    @date_issued,
                    @id_no,
                    @year_applied,
                    @control_no,
                    @id_type,
                    @pvc_no,
                    @age,
                    @print_count
                )
                """;

            await using var cmd = new MySqlCommand(insertSql, connection);
            AddUpsertParameters(cmd, record);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Inserted id-print record trans_no={TransNo} type={TranType} batch_no={BatchNo} app_no={AppNo}",
                record.TransNo, record.TranType, record.BatchNo, record.AppNo);
        }
    }

    private static void AddUpsertParameters(MySqlCommand cmd, IdPrint record)
    {
        cmd.Parameters.AddWithValue("@trans_no",     (object?)record.TransNo     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@trans_date",   (object?)record.TransDate   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@trans_time",   (object?)record.TransTime   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@batch_no",     (object?)record.BatchNo     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@batch_date",   (object?)record.BatchDate   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@app_no",       (object?)record.AppNo       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dob",          (object?)record.Dob         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@cp_no",        (object?)record.CpNo        ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@fullname",     (object?)record.Fullname    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@hcp_fullname", (object?)record.HcpFullname ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@address",      (object?)record.Address     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@hcp_cpno",     (object?)record.HcpCpno     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lname",        (object?)record.Lname       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@fname",        (object?)record.Fname       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mname",        (object?)record.Mname       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spa_docno",    (object?)record.SpaDocno    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@page_no",      (object?)record.PageNo      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@book_no",      (object?)record.BookNo      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@series_no",    (object?)record.SeriesNo    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spa_atty",     (object?)record.SpaAtty     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spa_dt_issue", (object?)record.SpaDtIssue  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@printed",      (object?)record.Printed     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@printed_dt",   (object?)record.PrintedDt   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@printed_tm",   (object?)record.PrintedTm   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tran_type",    (object?)record.TranType    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@image_data",   (object?)record.ImageData   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_code",      (object?)record.IdCode      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@sfx",          (object?)record.Sfx         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@photo",        (object?)record.Photo       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@gender",       (object?)record.Gender      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@date_issued",  (object?)record.DateIssued  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_no",        (object?)record.IdNo        ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@year_applied", (object?)record.YearApplied ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@control_no",   (object?)record.ControlNo   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_type",      (object?)record.IdType      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@pvc_no",       (object?)record.PvcNo       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@age",          (object?)record.Age         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@print_count",  (object?)record.PrintCount  ?? DBNull.Value);
    }

    private const string LastFullSyncKey = "last_full_sync_utc";

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
    /// Reads the UTC timestamp of the last successful FULL sync,
    /// or null if a full sync has never been recorded.
    /// </summary>
    public async Task<DateTime?> GetLastFullSyncUtcAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await EnsureSyncStateTableAsync(connection);

        const string sql = "SELECT state_value FROM sync_state WHERE state_key = @key";
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@key", LastFullSyncKey);

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
    /// Persists the UTC timestamp of a successful FULL sync.
    /// </summary>
    public async Task SetLastFullSyncUtcAsync(DateTime utc)
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
        cmd.Parameters.AddWithValue("@key", LastFullSyncKey);
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
    /// </summary>
    public async Task<long?> GetMaxTransNoAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT MAX(trans_no) FROM id_print";
        await using var cmd = new MySqlCommand(sql, connection);
        object? result = await cmd.ExecuteScalarAsync();

        if (result == null || result is DBNull)
            return null;

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Returns the trans_no values of records that have been printed by
    /// Instant ID (print_count > 0) but are not yet marked as printed.
    /// trans_no is used (not app_no) because OSCA/senior rows have no app_no.
    /// </summary>
    public async Task<List<long>> GetPrintedTransNosAsync()
    {
        var transNos = new List<long>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT trans_no
            FROM id_print
            WHERE trans_no > 0
              AND print_count > 0
              AND (printed IS NULL OR printed = 'no')
            """;

        await using var cmd = new MySqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
                transNos.Add(reader.GetInt64(0));
        }

        return transNos;
    }

    public async Task MarkPrintedAsync(long transNo)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE id_print
            SET printed = 'yes'
            WHERE trans_no = @trans_no
            """;

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@trans_no", transNo);
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        // MySqlConnection is disposed via `await using` in each method.
    }
}
