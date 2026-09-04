using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OpenEdgeBridge.Models;

namespace OpenEdgeBridge.Repositories;

public class MySqlRepository : IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<MySqlRepository> _logger;

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

        // Check if the record already exists by app_no to avoid burning
        // auto-increment IDs on every INSERT ... ON DUPLICATE KEY UPDATE cycle.
        // (InnoDB allocates the next auto-increment value even when the INSERT
        //  becomes an UPDATE, causing large gaps over repeated sync runs.)
        const string checkSql = "SELECT COUNT(1) FROM id_print WHERE app_no = @app_no";
        await using var checkCmd = new MySqlCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@app_no", (object?)record.AppNo ?? DBNull.Value);
        bool exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;

        if (exists)
        {
            const string updateSql = """
                UPDATE id_print
                SET
                    batch_no      = @batch_no,
                    batch_date    = @batch_date,
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
                    control_no    = @control_no
                WHERE app_no = @app_no
                """;

            await using var cmd = new MySqlCommand(updateSql, connection);
            AddUpsertParameters(cmd, record);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Updated id-print record app_no={AppNo}", record.AppNo);
        }
        else
        {
            const string insertSql = """
                INSERT INTO id_print
                (
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
                    print_count
                )
                VALUES
                (
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
                    @print_count
                )
                """;

            await using var cmd = new MySqlCommand(insertSql, connection);
            AddUpsertParameters(cmd, record);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Inserted id-print record batch_no={BatchNo} app_no={AppNo}", record.BatchNo, record.AppNo);
        }
    }

    private static void AddUpsertParameters(MySqlCommand cmd, IdPrint record)
    {
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
    /// Returns the highest batch_no currently stored in MySQL.
    /// Used for incremental sync — only fetch newer batches from OpenEdge.
    /// Each batch from OpenEdge contains ~100 records.
    /// </summary>
    public async Task<int?> GetMaxBatchNoAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT MAX(batch_no) FROM id_print";
        await using var cmd = new MySqlCommand(sql, connection);
        object? result = await cmd.ExecuteScalarAsync();

        if (result == null || result is DBNull)
            return null;

        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Returns the app_no values of records that have been printed by Instant ID
    /// (print_count > 0) but are not yet marked as printed.
    /// </summary>
    public async Task<List<int>> GetPrintedAppNosAsync()
    {
        var appNos = new List<int>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT app_no
            FROM id_print
            WHERE app_no > 0
              AND print_count > 0
              AND (printed IS NULL OR printed = 'no')
            """;

        await using var cmd = new MySqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
                appNos.Add(reader.GetInt32(0));
        }

        return appNos;
    }

    public async Task MarkPrintedAsync(int appNo)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE id_print
            SET printed = 'yes'
            WHERE app_no = @app_no
            """;

        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@app_no", appNo);
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        // MySqlConnection is disposed via `await using` in each method.
    }
}
