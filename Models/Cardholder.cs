namespace OpenEdgeBridge.Models;

public class Cardholder
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public int PrintCount { get; set; }
}
