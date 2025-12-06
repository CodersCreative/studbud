using Acascendia.Components;
using MudBlazor.Services;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using Acascendia.Models;

var builder = WebApplication.CreateBuilder(args);

var surreal =
SurrealDbOptions
  .Create()
  .WithEndpoint("http://127.0.0.1:8000")
  .WithNamespace("main")
  .WithDatabase("main")
  .Build();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddSingleton<Data>();
builder.Services.AddSurreal(surreal);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await InitializeDbAsync();

app.Run();

async Task InitializeDbAsync()
{
    var surrealDbClient = new SurrealDbClient(surreal);
    await DefineSchemaAsync(surrealDbClient);
}

async Task DefineSchemaAsync(ISurrealDbClient surrealDbClient)
{
    await surrealDbClient.RawQuery("""
DEFINE TABLE IF NOT EXISTS user SCHEMALESS;
DEFINE FIELD IF NOT EXISTS Username ON TABLE user TYPE string;
DEFINE FIELD IF NOT EXISTS Email ON TABLE user TYPE string;
DEFINE FIELD IF NOT EXISTS Password ON TABLE user TYPE string;

DEFINE TABLE IF NOT EXISTS class SCHEMALESS;
DEFINE FIELD IF NOT EXISTS Name ON TABLE class TYPE string;
DEFINE FIELD IF NOT EXISTS Users ON TABLE class TYPE Array<string>;
DEFINE FIELD IF NOT EXISTS Teachers ON TABLE class TYPE Array<string>;

DEFINE TABLE IF NOT EXISTS chat SCHEMALESS;
DEFINE FIELD IF NOT EXISTS Name ON TABLE class TYPE string;
DEFINE FIELD IF NOT EXISTS Users ON TABLE class TYPE Array<string>;
""");
}

public class Data
{
    public UserInfo? user {get; set;}

    public void UpdateUser(UserInfo u)
    {
        user = u;
    }
}