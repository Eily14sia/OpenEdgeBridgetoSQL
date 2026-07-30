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

    public async Task<List<IdPrint>> GetIdPrintsAsync()
    {
        var idPrints = new List<IdPrint>();

        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                "batch-no",
                "batch-date",
                "app-no",
                "dob",
                "cp-no",
                "fullname",
                "hcp-fullname",
                "address",
                "hcp-cpno",
                "lname",
                "fname",
                "mname",
                "spa-docno",
                "page-no",
                "book-no",
                "series-no",
                "spa-atty",
                "spa-dt-issue",
                "printed",
                "printed-dt",
                "printed-tm",
                "id-type",
                "image-data",
                "id-code"
            FROM "id-print"
            """;

        await using var command = new OdbcCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        int oBatchNo     = reader.GetOrdinal("batch-no");
        int oBatchDate   = reader.GetOrdinal("batch-date");
        int oAppNo       = reader.GetOrdinal("app-no");
        int oDob         = reader.GetOrdinal("dob");
        int oCpNo        = reader.GetOrdinal("cp-no");
        int oFullname    = reader.GetOrdinal("fullname");
        int oHcpFullname = reader.GetOrdinal("hcp-fullname");
        int oAddress     = reader.GetOrdinal("address");
        int oHcpCpno     = reader.GetOrdinal("hcp-cpno");
        int oLname       = reader.GetOrdinal("lname");
        int oFname       = reader.GetOrdinal("fname");
        int oMname       = reader.GetOrdinal("mname");
        int oSpaDocno    = reader.GetOrdinal("spa-docno");
        int oPageNo      = reader.GetOrdinal("page-no");
        int oBookNo      = reader.GetOrdinal("book-no");
        int oSeriesNo    = reader.GetOrdinal("series-no");
        int oSpaAtty     = reader.GetOrdinal("spa-atty");
        int oSpaDtIssue  = reader.GetOrdinal("spa-dt-issue");
        int oPrinted     = reader.GetOrdinal("printed");
        int oPrintedDt   = reader.GetOrdinal("printed-dt");
        int oPrintedTm   = reader.GetOrdinal("printed-tm");
        int oIdType      = reader.GetOrdinal("id-type");
        int oImageData   = reader.GetOrdinal("image-data");
        int oIdCode      = reader.GetOrdinal("id-code");

        while (await reader.ReadAsync())
        {
            idPrints.Add(new IdPrint
            {
                BatchNo     = reader.IsDBNull(oBatchNo)     ? null : reader.GetInt32(oBatchNo),
                BatchDate   = reader.IsDBNull(oBatchDate)   ? null : reader.GetDateTime(oBatchDate),
                AppNo       = reader.IsDBNull(oAppNo)       ? null : reader.GetInt32(oAppNo),
                Dob         = reader.IsDBNull(oDob)         ? null : reader.GetDateTime(oDob),
                CpNo        = reader.IsDBNull(oCpNo)        ? null : reader.GetString(oCpNo),
                Fullname    = reader.IsDBNull(oFullname)    ? null : reader.GetString(oFullname),
                HcpFullname = reader.IsDBNull(oHcpFullname) ? null : reader.GetString(oHcpFullname),
                Address     = reader.IsDBNull(oAddress)     ? null : reader.GetString(oAddress),
                HcpCpno     = reader.IsDBNull(oHcpCpno)     ? null : reader.GetString(oHcpCpno),
                Lname       = reader.IsDBNull(oLname)       ? null : reader.GetString(oLname),
                Fname       = reader.IsDBNull(oFname)       ? null : reader.GetString(oFname),
                Mname       = reader.IsDBNull(oMname)       ? null : reader.GetString(oMname),
                SpaDocno    = reader.IsDBNull(oSpaDocno)    ? null : reader.GetString(oSpaDocno),
                PageNo      = reader.IsDBNull(oPageNo)      ? null : reader.GetString(oPageNo),
                BookNo      = reader.IsDBNull(oBookNo)      ? null : reader.GetString(oBookNo),
                SeriesNo    = reader.IsDBNull(oSeriesNo)    ? null : reader.GetString(oSeriesNo),
                SpaAtty     = reader.IsDBNull(oSpaAtty)     ? null : reader.GetString(oSpaAtty),
                SpaDtIssue  = reader.IsDBNull(oSpaDtIssue)  ? null : reader.GetDateTime(oSpaDtIssue),
                Printed     = reader.IsDBNull(oPrinted)     ? null : (reader.GetBoolean(oPrinted) ? "yes" : "no"),
                PrintedDt   = reader.IsDBNull(oPrintedDt)   ? null : reader.GetDateTime(oPrintedDt),
                PrintedTm   = reader.IsDBNull(oPrintedTm)   ? null : reader.GetString(oPrintedTm),
                IdType      = reader.IsDBNull(oIdType)      ? null : reader.GetInt32(oIdType),
                ImageData   = reader.IsDBNull(oImageData)   ? null : (byte[])reader[oImageData],
                IdCode      = reader.IsDBNull(oIdCode)      ? null : reader.GetInt32(oIdCode)
            });
        }

        return idPrints;
    }

    public async Task MarkPrintedAsync(int appNo)
    {
        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE "id-print"
            SET "printed" = 'yes'
            WHERE "app-no" = ?
            """;

        await using var command = new OdbcCommand(sql, connection);
        command.Parameters.Add(new OdbcParameter("@appNo", OdbcType.Int) { Value = appNo });

        int rows = await command.ExecuteNonQueryAsync();
        if (rows > 0)
            _logger.LogDebug("Marked app-no={AppNo} as printed in OpenEdge", appNo);
    }

    public void Dispose()
    {
        // OdbcConnection is disposed via `await using` in each method.
    }
}
