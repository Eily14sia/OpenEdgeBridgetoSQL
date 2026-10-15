namespace OpenEdgeBridge.Models;

/// <summary>
/// Which kind(s) of "id-print" rows the bridge handles. The OpenEdge table
/// holds two ID kinds side by side, distinguished by "tran-type":
///   1 = RTC           (batched  — keyed by batch-no + app-no)
///   2 = OSCA / senior (single   — keyed by control-no + id-no)
/// Configured via Sync:Client ("Both" | "RTC" | "OSCA") in appsettings.json;
/// also overridable with the Sync__Client environment variable.
/// </summary>
public enum IdPrintKind
{
    /// <summary>Sync both RTC and OSCA rows (default).</summary>
    Both = 0,

    /// <summary>Sync only tran-type = 1 (RTC) rows.</summary>
    Rtc = 1,

    /// <summary>Sync only tran-type = 2 (OSCA / senior) rows.</summary>
    Osca = 2
}

public static class IdPrintKindExtensions
{
    public const int TranTypeRtc = 1;
    public const int TranTypeOsca = 2;

    /// <summary>
    /// Parses a client name from configuration. Accepts "Both", "RTC", "OSCA"
    /// (case-insensitive), plus the aliases "SENIOR"/"2" for OSCA and "1" for
    /// RTC. Any unrecognised/blank value falls back to <see cref="IdPrintKind.Both"/>.
    /// </summary>
    public static IdPrintKind ParseClient(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return IdPrintKind.Both;

        return value.Trim().ToUpperInvariant() switch
        {
            "RTC" or "1"                 => IdPrintKind.Rtc,
            "OSCA" or "SENIOR" or "2"    => IdPrintKind.Osca,
            _                            => IdPrintKind.Both
        };
    }

    /// <summary>
    /// The OpenEdge "tran-type" value to filter on, or <c>null</c> for Both.
    /// </summary>
    public static int? TranTypeFilter(this IdPrintKind kind) => kind switch
    {
        IdPrintKind.Rtc  => TranTypeRtc,
        IdPrintKind.Osca => TranTypeOsca,
        _                => null
    };

    /// <summary>Human-readable client name for logs.</summary>
    public static string Describe(this IdPrintKind kind) => kind switch
    {
        IdPrintKind.Rtc  => "RTC",
        IdPrintKind.Osca => "OSCA",
        _                => "Both"
    };
}
