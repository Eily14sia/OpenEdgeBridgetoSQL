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

        // Use INSERT ... ON DUPLICATE KEY UPDATE keyed on app_no.
        // If the same app_no already exists, all other columns are refreshed.
        const string sql = """
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
                id_type,
                image_data,
                id_code,
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
                @id_type,
                @image_data,
                @id_code,
                @print_count
            )
            ON DUPLICATE KEY UPDATE
                batch_date    = VALUES(batch_date),
                dob           = VALUES(dob),
                cp_no         = VALUES(cp_no),
                fullname      = VALUES(fullname),
                hcp_fullname  = VALUES(hcp_fullname),
                address       = VALUES(address),
                hcp_cpno      = VALUES(hcp_cpno),
                lname         = VALUES(lname),
                fname         = VALUES(fname),
                mname         = VALUES(mname),
                spa_docno     = VALUES(spa_docno),
                page_no       = VALUES(page_no),
                book_no       = VALUES(book_no),
                series_no     = VALUES(series_no),
                spa_atty      = VALUES(spa_atty),
                spa_dt_issue  = VALUES(spa_dt_issue),
                printed       = VALUES(printed),
                printed_dt    = VALUES(printed_dt),
                printed_tm    = VALUES(printed_tm),
                id_type       = VALUES(id_type),
                image_data    = VALUES(image_data),
                id_code       = VALUES(id_code)
            """;

        await using var cmd = new MySqlCommand(sql, connection);
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
        cmd.Parameters.AddWithValue("@id_type",      (object?)record.IdType      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@image_data",   (object?)record.ImageData   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id_code",      (object?)record.IdCode      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@print_count",  (object?)record.PrintCount  ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        _logger.LogDebug("Upserted id-print record batch_no={BatchNo} app_no={AppNo}", record.BatchNo, record.AppNo);
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
