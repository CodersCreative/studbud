using SurrealDb.Net.Models;

namespace Acascendia.Models;

public class Class : Record
{
    public string[]? Name {get; set;}
    public string[]? Users {get; set;}
    public string? Teachers {get; set;}
}