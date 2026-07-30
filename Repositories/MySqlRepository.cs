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

    public async Task UpsertCardholderAsync(Cardholder cardholder)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check if the record already exists by openedge_id before deciding
        // whether to INSERT or UPDATE. This avoids duplicates even if the
        // MySQL table lacks a UNIQUE constraint on openedge_id.
        const string checkSql = "SELECT COUNT(1) FROM cardholders WHERE openedge_id = @id";
        await using var checkCmd = new MySqlCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@id", cardholder.Id);
        long exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());

        if (exists > 0)
        {
            const string updateSql = """
                UPDATE cardholders
                SET
                    employee_id = @employee,
                    first_name  = @first,
                    last_name   = @last,
                    department  = @department,
                    position    = @position,
                    print_count = @count,
                    synced_at   = NOW()
                WHERE openedge_id = @id
                """;

            await using var updateCmd = new MySqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@id", cardholder.Id);
            updateCmd.Parameters.AddWithValue("@employee", cardholder.EmployeeId);
            updateCmd.Parameters.AddWithValue("@first", cardholder.FirstName);
            updateCmd.Parameters.AddWithValue("@last", cardholder.LastName);
            updateCmd.Parameters.AddWithValue("@department", cardholder.Department);
            updateCmd.Parameters.AddWithValue("@position", cardholder.Position);
            updateCmd.Parameters.AddWithValue("@count", cardholder.PrintCount);

            await updateCmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Updated existing cardholder {EmployeeId} (openedge_id={Id})", cardholder.EmployeeId, cardholder.Id);
        }
        else
        {
            const string insertSql = """
                INSERT INTO cardholders
                (
                    openedge_id,
                    employee_id,
                    first_name,
                    last_name,
                    department,
                    position,
                    print_count,
                    synced_at
                )
                VALUES
                (
                    @id,
                    @employee,
                    @first,
                    @last,
                    @department,
                    @position,
                    @count,
                    NOW()
                )
                """;

            await using var insertCmd = new MySqlCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@id", cardholder.Id);
            insertCmd.Parameters.AddWithValue("@employee", cardholder.EmployeeId);
            insertCmd.Parameters.AddWithValue("@first", cardholder.FirstName);
            insertCmd.Parameters.AddWithValue("@last", cardholder.LastName);
            insertCmd.Parameters.AddWithValue("@department", cardholder.Department);
            insertCmd.Parameters.AddWithValue("@position", cardholder.Position);
            insertCmd.Parameters.AddWithValue("@count", cardholder.PrintCount);

            await insertCmd.ExecuteNonQueryAsync();
            _logger.LogDebug("Inserted new cardholder {EmployeeId} (openedge_id={Id})", cardholder.EmployeeId, cardholder.Id);
        }
    }

    public async Task<List<Cardholder>> GetPendingPrintsAsync()
    {
        var pending = new List<Cardholder>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Returns all cardholders from MySQL.
        // The SyncService will compare print_count values against OpenEdge
        // to determine which records need their print count synced back.
        const string sql = """
            SELECT
                openedge_id,
                employee_id,
                first_name,
                last_name,
                department,
                position,
                print_count
            FROM cardholders
            """;

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        int ordId = reader.GetOrdinal("openedge_id");
        int ordEmployeeId = reader.GetOrdinal("employee_id");
        int ordFirstName = reader.GetOrdinal("first_name");
        int ordLastName = reader.GetOrdinal("last_name");
        int ordDepartment = reader.GetOrdinal("department");
        int ordPosition = reader.GetOrdinal("position");
        int ordPrintCount = reader.GetOrdinal("print_count");

        while (await reader.ReadAsync())
        {
            pending.Add(new Cardholder
            {
                Id = reader.GetInt32(ordId),
                EmployeeId = reader.IsDBNull(ordEmployeeId)
                    ? string.Empty
                    : reader.GetString(ordEmployeeId),
                FirstName = reader.IsDBNull(ordFirstName)
                    ? string.Empty
                    : reader.GetString(ordFirstName),
                LastName = reader.IsDBNull(ordLastName)
                    ? string.Empty
                    : reader.GetString(ordLastName),
                Department = reader.IsDBNull(ordDepartment)
                    ? string.Empty
                    : reader.GetString(ordDepartment),
                Position = reader.IsDBNull(ordPosition)
                    ? string.Empty
                    : reader.GetString(ordPosition),
                PrintCount = reader.IsDBNull(ordPrintCount)
                    ? 0
                    : reader.GetInt32(ordPrintCount)
            });
        }

        return pending;
    }

    public void Dispose()
    {
        // MySqlConnection is disposed via `await using` in each method.
    }
}
