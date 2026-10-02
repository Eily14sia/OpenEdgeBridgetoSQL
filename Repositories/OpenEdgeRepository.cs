using System.Data.Odbc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenEdgeBridge.Models;

namespace OpenEdgeBridge.Repositories;

public class OpenEdgeRepository : IDisposable
{
    private readonly string _connectionString;
    private readonly string _connectionStringRead;
    private readonly ILogger<OpenEdgeRepository> _logger;

    public OpenEdgeRepository(IConfiguration configuration, ILogger<OpenEdgeRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("OpenEdge")
            ?? throw new InvalidOperationException("OpenEdge connection string not found.");
        _connectionStringRead = configuration.GetConnectionString("OpenEdgeRead")
            ?? _connectionString;
        _logger = logger;
    }

    public async Task<List<IdPrint>> GetIdPrintsAsync()
    {
        // Full sync — no trans_no filter.
        return await GetIdPrintsInternalAsync(null);
    }

    /// <summary>
    /// Incremental sync — only fetch records whose trans-no (the table-wide
    /// PRIMARY key, id-idx) is newer than the highest trans_no already in MySQL.
    /// trans-no exists on every row (RTC and OSCA/senior alike), unlike batch-no
    /// which is NULL for OSCA rows, so it is the safe universal watermark.
    /// Pass null for a full sync.
    /// </summary>
    public async Task<List<IdPrint>> GetIdPrintsNewerThanAsync(long? minTransNo)
    {
        return await GetIdPrintsInternalAsync(minTransNo);
    }

    private async Task<List<IdPrint>> GetIdPrintsInternalAsync(long? minTransNo)
    {
        var idPrints = new List<IdPrint>();

        await using var connection = new OdbcConnection(_connectionStringRead);
        await connection.OpenAsync();

        string sql;
        if (minTransNo.HasValue)
        {
            sql = """
                SELECT
                    "trans-no",
                    "trans-date",
                    "trans-time",
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
                    "tran-type",
                    "image-data",
                    "id-code",
                    "sfx",
                    "photo",
                    "gender",
                    "date-issued",
                    "id-no",
                    "year-applied",
                    "control-no",
                    "id-type",
                    "PVC-No",
                    "age"
                FROM pub."id-print"
                WHERE "trans-no" > ?
                """;
        }
        else
        {
            sql = """
                SELECT
                    "trans-no",
                    "trans-date",
                    "trans-time",
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
                    "tran-type",
                    "image-data",
                    "id-code",
                    "sfx",
                    "photo",
                    "gender",
                    "date-issued",
                    "id-no",
                    "year-applied",
                    "control-no",
                    "id-type",
                    "PVC-No",
                    "age"
                FROM pub."id-print"
                """;
        }

        await using var command = new OdbcCommand(sql, connection);
        if (minTransNo.HasValue)
        {
            command.Parameters.Add(new OdbcParameter("@minTransNo", OdbcType.BigInt) { Value = minTransNo.Value });
        }

        await using var reader = await command.ExecuteReaderAsync();

        int oTransNo     = reader.GetOrdinal("trans-no");
        int oTransDate   = reader.GetOrdinal("trans-date");
        int oTransTime   = reader.GetOrdinal("trans-time");
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
        int oTranType    = reader.GetOrdinal("tran-type");
        int oImageData   = reader.GetOrdinal("image-data");
        int oIdCode      = reader.GetOrdinal("id-code");
        int oSfx         = reader.GetOrdinal("sfx");
        int oPhoto       = reader.GetOrdinal("photo");
        int oGender      = reader.GetOrdinal("gender");
        int oDateIssued  = reader.GetOrdinal("date-issued");
        int oIdNo        = reader.GetOrdinal("id-no");
        int oYearApplied = reader.GetOrdinal("year-applied");
        int oControlNo   = reader.GetOrdinal("control-no");
        int oIdType      = reader.GetOrdinal("id-type");
        int oPvcNo       = reader.GetOrdinal("PVC-No");
        int oAge         = reader.GetOrdinal("age");

        while (await reader.ReadAsync())
        {
            idPrints.Add(new IdPrint
            {
                TransNo     = reader.IsDBNull(oTransNo)     ? null : reader.GetInt64(oTransNo),
                TransDate   = reader.IsDBNull(oTransDate)   ? null : reader.GetDateTime(oTransDate),
                TransTime   = reader.IsDBNull(oTransTime)   ? null : reader.GetString(oTransTime),
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
                Printed     = reader.IsDBNull(oPrinted)     ? null : ReadLogicalAsYesNo(reader, oPrinted),
                PrintedDt   = reader.IsDBNull(oPrintedDt)   ? null : reader.GetDateTime(oPrintedDt),
                PrintedTm   = reader.IsDBNull(oPrintedTm)   ? null : reader.GetString(oPrintedTm),
                TranType    = reader.IsDBNull(oTranType)    ? null : reader.GetInt32(oTranType),
                ImageData   = reader.IsDBNull(oImageData)   ? null : (byte[])reader[oImageData],
                IdCode      = reader.IsDBNull(oIdCode)      ? null : reader.GetInt32(oIdCode),
                Sfx         = reader.IsDBNull(oSfx)         ? null : reader.GetString(oSfx),
                Photo       = ReadBlob(reader, oPhoto),
                Gender      = reader.IsDBNull(oGender)      ? null : reader.GetInt32(oGender),
                DateIssued  = reader.IsDBNull(oDateIssued)  ? null : reader.GetDateTime(oDateIssued),
                IdNo        = reader.IsDBNull(oIdNo)        ? null : reader.GetString(oIdNo),
                YearApplied = reader.IsDBNull(oYearApplied) ? null : reader.GetInt32(oYearApplied),
                ControlNo   = reader.IsDBNull(oControlNo)   ? null : reader.GetInt32(oControlNo),
                IdType      = reader.IsDBNull(oIdType)      ? null : reader.GetInt32(oIdType),
                PvcNo       = reader.IsDBNull(oPvcNo)       ? null : reader.GetString(oPvcNo),
                Age         = reader.IsDBNull(oAge)         ? null : reader.GetInt32(oAge)
            });
        }

        return idPrints;
    }

    /// <summary>
    /// Safely reads an OpenEdge LOGICAL field regardless of how the ODBC driver
    /// represents it (boolean, integer 1/0, or string "yes"/"no").
    /// </summary>
    private static string? ReadLogicalAsYesNo(System.Data.Common.DbDataReader reader, int ordinal)
    {
        // Attempt 1: native boolean (preferred path for drivers that support it)
        try
        {
            return reader.GetBoolean(ordinal) ? "yes" : "no";
        }
        catch (InvalidCastException)
        {
            // Fall through to string-based detection
        }

        // Attempt 2: read as object and detect type at runtime
        object raw = reader.GetValue(ordinal);
        if (raw == DBNull.Value)
            return null;

        return raw switch
        {
            bool b            => b ? "yes" : "no",
            int i             => i != 0 ? "yes" : "no",
            short s           => s != 0 ? "yes" : "no",
            long l            => l != 0 ? "yes" : "no",
            string str        => str.Trim().ToLowerInvariant() switch
            {
                "yes" or "true" or "1" or "y" => "yes",
                _                             => "no"
            },
            _ => "no"
        };
    }

    /// <summary>
    /// Reads an OpenEdge BLOB field (e.g. "photo") as a byte array.
    /// The DataDirect ODBC driver can return byte[] or another binary
    /// representation depending on the driver version.
    /// </summary>
    private static byte[]? ReadBlob(System.Data.Common.DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return null;

        object raw = reader.GetValue(ordinal);
        return raw switch
        {
            byte[] bytes => bytes,
            _            => null
        };
    }

    public async Task MarkPrintedAsync(long transNo)
    {
        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            UPDATE pub."id-print"
            SET "printed" = 1
            WHERE "trans-no" = ?
            """;

        await using var command = new OdbcCommand(sql, connection);
        command.Parameters.Add(new OdbcParameter("@transNo", OdbcType.BigInt) { Value = transNo });

        int rows = await command.ExecuteNonQueryAsync();
        if (rows > 0)
            _logger.LogDebug("Marked trans-no={TransNo} as printed in OpenEdge", transNo);
    }

    public void Dispose()
    {
        // OdbcConnection is disposed via `await using` in each method.
    }
}
