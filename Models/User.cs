using SurrealDb.Net.Models;

namespace Acascendia.Models;

public class UserInfo : Record
{
    public string? Username {get; set;}
    public string? Email {get; set;}
    public string? Password {get; set;}
}

public class User : Record
{
    public string? Username {get; set;}
    public string? Password {get; set;}
}