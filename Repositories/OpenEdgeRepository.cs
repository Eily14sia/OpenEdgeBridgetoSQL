using System.Data.Odbc;
using System.Globalization;
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

    public async Task<List<IdPrint>> GetIdPrintsAsync(IdPrintKind kind = IdPrintKind.Both)
    {
        // Full sync — no trans_no filter.
        return await GetIdPrintsInternalAsync(null, kind);
    }

    /// <summary>
    /// Incremental sync — only fetch records whose trans-no (the table-wide
    /// PRIMARY key, id-idx) is newer than the highest trans_no already in MySQL.
    /// trans-no exists on every row (RTC and OSCA/senior alike), unlike batch-no
    /// which is NULL for OSCA rows, so it is the safe universal watermark.
    /// Pass null for a full sync. <paramref name="kind"/> restricts both the rows
    /// read (tran-type) and the columns projected to a single client.
    /// </summary>
    public async Task<List<IdPrint>> GetIdPrintsNewerThanAsync(long? minTransNo, IdPrintKind kind = IdPrintKind.Both)
    {
        return await GetIdPrintsInternalAsync(minTransNo, kind);
    }

    // --- Client-specific column projection (per the RTC / OSCA field lists) ----
    // The common columns exist on both kinds; the "*Only*" lists are the extra
    // columns each kind populates. Selecting only the relevant columns keeps the
    // other kind's BLOB/RAW fields (photo, image-data) out of the result set.

    private static readonly string[] CommonColumns =
    {
        "\"trans-no\"", "\"trans-date\"", "\"trans-time\"", "\"tran-type\"",
        "\"address\"", "\"cp-no\"", "\"dob\"",
        "\"lname\"", "\"fname\"", "\"mname\"", "\"sfx\"", "\"fullname\"",
        "\"gender\"", "\"id-type\"", "\"image-data\"", "\"photo\"",
        "\"printed\"", "\"printed-dt\"", "\"printed-tm\"", "\"PVC-No\""
    };

    private static readonly string[] RtcOnlyColumns =
    {
        "\"batch-no\"", "\"batch-date\"", "\"app-no\"", "\"book-no\"", "\"page-no\"",
        "\"series-no\"", "\"spa-docno\"", "\"spa-atty\"", "\"spa-dt-issue\"",
        "\"id-code\"", "\"hcp-cpno\"", "\"hcp-fullname\""
    };

    private static readonly string[] OscaOnlyColumns =
    {
        "\"age\"", "\"control-no\"", "\"date-issued\"", "\"id-no\"", "\"year-applied\""
    };

    private static List<string> ColumnsFor(IdPrintKind kind) => kind switch
    {
        IdPrintKind.Rtc  => CommonColumns.Concat(RtcOnlyColumns).ToList(),
        IdPrintKind.Osca => CommonColumns.Concat(OscaOnlyColumns).ToList(),
        _                => CommonColumns.Concat(RtcOnlyColumns).Concat(OscaOnlyColumns).ToList()
    };

    private async Task<List<IdPrint>> GetIdPrintsInternalAsync(long? minTransNo, IdPrintKind kind)
    {
        var idPrints = new List<IdPrint>();

        await using var connection = new OdbcConnection(_connectionStringRead);
        await connection.OpenAsync();

        List<string> columns = ColumnsFor(kind);
        int? tranTypeFilter = kind.TranTypeFilter();

        // Build the WHERE clause from whichever filters apply.
        // NOTE: the parameter order below must match the order of '?' here.
        var whereParts = new List<string>();
        if (minTransNo.HasValue)
            whereParts.Add("\"trans-no\" > ?");
        if (tranTypeFilter.HasValue)
            whereParts.Add("\"tran-type\" = ?");

        var sql = new System.Text.StringBuilder();
        sql.Append("SELECT\n    ");
        sql.Append(string.Join(",\n    ", columns));
        sql.Append("\nFROM pub.\"id-print\"");
        if (whereParts.Count > 0)
            sql.Append("\nWHERE ").Append(string.Join(" AND ", whereParts));

        await using var command = new OdbcCommand(sql.ToString(), connection);
        if (minTransNo.HasValue)
        {
            command.Parameters.Add(new OdbcParameter("@minTransNo", OdbcType.BigInt) { Value = minTransNo.Value });
        }
        if (tranTypeFilter.HasValue)
        {
            command.Parameters.Add(new OdbcParameter("@tranType", OdbcType.Int) { Value = tranTypeFilter.Value });
        }

        await using var reader = await command.ExecuteReaderAsync();

        // Map only the columns actually selected, by name — ordinals differ
        // between client modes because the projection differs.
        var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (string column in columns)
        {
            string name = column.Trim('"');
            ordinals[name] = reader.GetOrdinal(name);
        }

        object? Value(string name) =>
            ordinals.TryGetValue(name, out int ordinal) && !reader.IsDBNull(ordinal)
                ? reader.GetValue(ordinal)
                : null;

        while (await reader.ReadAsync())
        {
            idPrints.Add(new IdPrint
            {
                TransNo     = AsLong(Value("trans-no")),
                TransDate   = AsDate(Value("trans-date")),
                TransTime   = AsString(Value("trans-time")),
                BatchNo     = AsInt(Value("batch-no")),
                BatchDate   = AsDate(Value("batch-date")),
                AppNo       = AsInt(Value("app-no")),
                Dob         = AsDate(Value("dob")),
                CpNo        = AsString(Value("cp-no")),
                Fullname    = AsString(Value("fullname")),
                HcpFullname = AsString(Value("hcp-fullname")),
                Address     = AsString(Value("address")),
                HcpCpno     = AsString(Value("hcp-cpno")),
                Lname       = AsString(Value("lname")),
                Fname       = AsString(Value("fname")),
                Mname       = AsString(Value("mname")),
                SpaDocno    = AsString(Value("spa-docno")),
                PageNo      = AsString(Value("page-no")),
                BookNo      = AsString(Value("book-no")),
                SeriesNo    = AsString(Value("series-no")),
                SpaAtty     = AsString(Value("spa-atty")),
                SpaDtIssue  = AsDate(Value("spa-dt-issue")),
                Printed     = AsYesNo(Value("printed")),
                PrintedDt   = AsDate(Value("printed-dt")),
                PrintedTm   = AsString(Value("printed-tm")),
                TranType    = AsInt(Value("tran-type")),
                ImageData   = AsBytes(Value("image-data")),
                IdCode      = AsInt(Value("id-code")),
                Sfx         = AsString(Value("sfx")),
                Photo       = AsBytes(Value("photo")),
                Gender      = AsInt(Value("gender")),
                DateIssued  = AsDate(Value("date-issued")),
                IdNo        = AsString(Value("id-no")),
                YearApplied = AsInt(Value("year-applied")),
                ControlNo   = AsInt(Value("control-no")),
                IdType      = AsInt(Value("id-type")),
                PvcNo       = AsString(Value("PVC-No")),
                Age         = AsInt(Value("age"))
            });
        }

        return idPrints;
    }

    // --- Value converters ------------------------------------------------------
    // The ODBC driver may surface the same OpenEdge field as different CLR types
    // depending on the driver version / column type, so each converter accepts
    // whatever the reader returned (or null for a missing/DBNull column).

    private static long? AsLong(object? v) => v switch
    {
        null     => null,
        long l   => l,
        int i    => i,
        short s  => s,
        byte b   => b,
        decimal d => (long)d,
        _        => Convert.ToInt64(v, CultureInfo.InvariantCulture)
    };

    private static int? AsInt(object? v) => v switch
    {
        null     => null,
        int i    => i,
        short s  => s,
        byte b   => b,
        long l   => (int)l,
        decimal d => (int)d,
        _        => Convert.ToInt32(v, CultureInfo.InvariantCulture)
    };

    private static string? AsString(object? v) => v switch
    {
        null   => null,
        string s => s,
        _      => v.ToString()
    };

    private static DateTime? AsDate(object? v) => v switch
    {
        null      => null,
        DateTime dt => dt,
        string s when DateTime.TryParse(s, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime parsed) => parsed,
        _         => Convert.ToDateTime(v, CultureInfo.InvariantCulture)
    };

    /// <summary>
    /// Safely converts an OpenEdge LOGICAL field to "yes"/"no" regardless of how
    /// the ODBC driver represents it (boolean, integer 1/0, or string "yes"/"no").
    /// </summary>
    private static string? AsYesNo(object? v) => v switch
    {
        null     => null,
        bool b   => b ? "yes" : "no",
        int i    => i != 0 ? "yes" : "no",
        short s  => s != 0 ? "yes" : "no",
        long l   => l != 0 ? "yes" : "no",
        string str => str.Trim().ToLowerInvariant() switch
        {
            "yes" or "true" or "1" or "y" => "yes",
            _                             => "no"
        },
        _ => "no"
    };

    /// <summary>
    /// Reads an OpenEdge BLOB / raw field (e.g. "photo", "image-data") as a byte
    /// array. The DataDirect ODBC driver can return byte[] or another binary
    /// representation depending on the driver version.
    /// </summary>
    private static byte[]? AsBytes(object? v) => v as byte[];

    /// <summary>
    /// Marks an OpenEdge row as printed. Besides <c>"printed" = 1</c>, the print
    /// date/time are ALWAYS stamped with the bridge machine's local time
    /// (<paramref name="printedAt"/>) — whatever Instant ID wrote into MySQL is
    /// deliberately ignored. OpenEdge's own <c>printed-dt</c>/<c>printed-tm</c>
    /// are always empty for these rows, so there is nothing to read or preserve.
    /// "PVC-No" is intentionally NOT written back. Photo/image-data are written
    /// only when they carry content.
    /// </summary>
    public async Task MarkPrintedAsync(
        long transNo,
        DateTime printedAt,
        byte[]? photo = null,
        byte[]? imageData = null)
    {
        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync();

        var sets = new List<string> { "\"printed\" = 1" };
        var extraParameters = new List<OdbcParameter>();

        // printed-dt: always this machine's local date (Instant ID's stamp is
        // ignored — print_count > 0 is what triggers the back-sync).
        sets.Add("\"printed-dt\" = ?");
        extraParameters.Add(new OdbcParameter("@printedDt", OdbcType.Date)
        {
            Value = printedAt.Date
        });

        // printed-tm: always this machine's local time.
        sets.Add("\"printed-tm\" = ?");
        extraParameters.Add(new OdbcParameter("@printedTm", OdbcType.VarChar)
        {
            Value = printedAt.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
        });

        if (photo is { Length: > 0 })
        {
            sets.Add("\"photo\" = ?");
            // OdbcType.Image maps to SQL_LONGVARBINARY — appropriate for the
            // OpenEdge "photo" BLOB (there is no LongVarBinary member).
            extraParameters.Add(new OdbcParameter("@photo", OdbcType.Image) { Value = photo });
        }
        if (imageData is { Length: > 0 })
        {
            sets.Add("\"image-data\" = ?");
            extraParameters.Add(new OdbcParameter("@imageData", OdbcType.VarBinary) { Value = imageData });
        }

        string sql = $"""
            UPDATE pub."id-print"
            SET {string.Join(", ", sets)}
            WHERE "trans-no" = ?
            """;

        await using var command = new OdbcCommand(sql, connection);
        foreach (var parameter in extraParameters)
            command.Parameters.Add(parameter);
        command.Parameters.Add(new OdbcParameter("@transNo", OdbcType.BigInt) { Value = transNo });

        int rows = await command.ExecuteNonQueryAsync();
        if (rows > 0)
            _logger.LogDebug(
                "Marked trans-no={TransNo} as printed in OpenEdge (extra fields carried back: {ExtraCount})",
                transNo, extraParameters.Count);
    }

    public void Dispose()
    {
        // OdbcConnection is disposed via `await using` in each method.
    }
}
