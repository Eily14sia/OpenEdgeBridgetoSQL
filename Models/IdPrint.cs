namespace OpenEdgeBridge.Models;

public class IdPrint
{
    public int? BatchNo { get; set; }
    public DateTime? BatchDate { get; set; }
    public int? AppNo { get; set; }
    public DateTime? Dob { get; set; }
    public string? CpNo { get; set; }
    public string? Fullname { get; set; }
    public string? HcpFullname { get; set; }
    public string? Address { get; set; }
    public string? HcpCpno { get; set; }
    public string? Lname { get; set; }
    public string? Fname { get; set; }
    public string? Mname { get; set; }
    public string? SpaDocno { get; set; }
    public string? PageNo { get; set; }
    public string? BookNo { get; set; }
    public string? SeriesNo { get; set; }
    public string? SpaAtty { get; set; }
    public DateTime? SpaDtIssue { get; set; }
    public string? Printed { get; set; }
    public DateTime? PrintedDt { get; set; }
    public string? PrintedTm { get; set; }
    public int? IdType { get; set; }
    public byte[]? ImageData { get; set; }
    public int? IdCode { get; set; }
    public int? PrintCount { get; set; }
}
