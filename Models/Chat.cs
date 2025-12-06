using SurrealDb.Net.Models;

namespace Acascendia.Models;

public class Chat : Record
{
    public string? Name {get; set;}
    public string[]? Users {get; set;}
}