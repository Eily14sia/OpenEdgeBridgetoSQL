proenv>proserve C:\OpenEdgeDatabases\instantID -S 20940 -ServerType Both -n 51 -SQLTrc 3
OpenEdge Release 12.8 as of Thu Dec 21 10:17:00 EST 2023
16:02:06 SERVER   : You have not supplied a parameter for argument -L. (1403)
proenv>using System.Data.Odbc;

var connStr = "DSN=OpenEdgeInstantID;Uid=admin;";

try
{
    using var conn = new OdbcConnection(connStr);
    conn.Open();
    Console.WriteLine("Connected OK");

    using var cmd = new OdbcCommand("SELECT COUNT(*) FROM pub.\"id-print\"", conn);
    var result = cmd.ExecuteScalar();
    Console.WriteLine($"Row count: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
