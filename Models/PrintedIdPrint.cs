namespace OpenEdgeBridge.Models;

/// <summary>
/// A row awaiting the "printed" back-sync (MySQL → OpenEdge). Besides the key
/// (<see cref="TransNo"/>), it carries the image fields Entrust Instant ID may
/// fill in during printing so they can be written back to OpenEdge too. Optional
/// values are only pushed when they actually carry content, so an empty/NULL
/// MySQL value never blanks out what OpenEdge already holds.
/// The printed date/time are NOT carried over — the back-sync stamps them with
/// the bridge machine's local time.
/// </summary>
public class PrintedIdPrint
{
    public long TransNo { get; set; }

    // NOTE: "PVC-No" is deliberately NOT carried back. In MySQL pvc_no always
    // holds a value, so writing it would overwrite OpenEdge's PVC-No on every
    // back-sync. Only the fields below are pushed back.

    /// <summary>Maps to OpenEdge <c>"photo"</c> (BLOB).</summary>
    public byte[]? Photo { get; set; }

    /// <summary>Maps to OpenEdge <c>"image-data"</c> (VARBINARY).</summary>
    public byte[]? ImageData { get; set; }
}
