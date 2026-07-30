using System.Data;
using System.Data.Odbc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenEdgeBridge.Models;

namespace OpenEdgeBridge.Repositories;

public class OpenEdgeRepository : IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<OpenEdgeRepository> _logger;

    public OpenEdgeRepository(IConfiguration configuration, ILogger<OpenEdgeRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("OpenEdge")
            ?? throw new InvalidOperationException("OpenEdge connection string not found.");
        _logger = logger;
    }

    public async Task<List<Cardholder>> GetCardholdersAsync()
    {
        var cardholders = new List<Cardholder>();

        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                ID,
                EMPLOYEE_ID,
                FIRST_NAME,
                LAST_NAME,
                DEPARTMENT,
                POSITION,
                PRINT_COUNT
            FROM PUB.cardholder
            """;

        await using var command = new OdbcCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            cardholders.Add(new Cardholder
            {
                Id = reader.GetInt32("ID"),
                EmployeeId = reader.IsDBNull("EMPLOYEE_ID")
                    ? string.Empty
                    : reader.GetString("EMPLOYEE_ID"),
                FirstName = reader.IsDBNull("FIRST_NAME")
                    ? string.Empty
                    : reader.GetString("FIRST_NAME"),
                LastName = reader.IsDBNull("LAST_NAME")
                    ? string.Empty
                    : reader.GetString("LAST_NAME"),
                Department = reader.IsDBNull("DEPARTMENT")
                    ? string.Empty
                    : reader.GetString("DEPARTMENT"),
                Position = reader.IsDBNull("POSITION")
                    ? string.Empty
                    : reader.GetString("POSITION"),
                PrintCount = reader.IsDBNull("PRINT_COUNT")
                    ? 0
                    : reader.GetInt32("PRINT_COUNT")
            });
        }

        return cardholders;
    }

    public async Task UpdatePrintCountAsync(int id)
    {
        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE PUB.cardholder
            SET PRINT_COUNT = PRINT_COUNT + 1
            WHERE ID = ?
            """;

        await using var command = new OdbcCommand(sql, connection);
        command.Parameters.Add(new OdbcParameter("@id", OdbcType.Int) { Value = id });

        int rowsAffected = await command.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
        {
            _logger.LogWarning("No cardholder found with ID {CardholderId} to update print count.", id);
        }
    }

    public void Dispose()
    {
        // OdbcConnection is disposed via `await using` in each method.
        // Odbc does not require explicit connection pooling cleanup.
    }
}
