using Microsoft.AspNetCore.ResponseCompression;
using studbud.Hubs;
using OllamaSharp;
using SurrealDb.Net;

var builder = WebApplication.CreateBuilder(args);

var surreal =
SurrealDbOptions
  .Create()
  .WithEndpoint("http://127.0.0.1:8000")
  .WithNamespace("main")
  .WithDatabase("main")
  .Build();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
   opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       [ "application/octet-stream" ]);
});
builder.Services.AddSurreal(surreal);

string? origins = "origins";
builder.Services.AddCors(options =>
{
     options.AddPolicy(origins,
                           policy =>
                           {
                                policy.AllowAnyHeader();
                                policy.AllowAnyMethod();
                                policy.AllowAnyOrigin();
                           });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors(origins);
app.UseResponseCompression();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapHub<AppHub>("/apphub");

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
DEFINE FIELD IF NOT EXISTS username ON TABLE user TYPE string;
DEFINE FIELD IF NOT EXISTS email ON TABLE user TYPE string;
DEFINE FIELD IF NOT EXISTS password ON TABLE user TYPE string;

DEFINE TABLE IF NOT EXISTS class SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE class TYPE string;
DEFINE FIELD IF NOT EXISTS code ON TABLE class TYPE string;
DEFINE FIELD IF NOT EXISTS userIds ON TABLE class TYPE array<string>;
DEFINE FIELD IF NOT EXISTS teacherIds ON TABLE class TYPE array<string>;

DEFINE TABLE IF NOT EXISTS chat SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE class TYPE option<string>;
DEFINE FIELD IF NOT EXISTS userIds ON TABLE class TYPE array<string>;

DEFINE TABLE IF NOT EXISTS message SCHEMALESS;
DEFINE FIELD IF NOT EXISTS date ON TABLE message TYPE datetime DEFAULT time::now();
DEFINE FIELD IF NOT EXISTS parentId ON TABLE message TYPE string;
DEFINE FIELD IF NOT EXISTS text ON TABLE message TYPE string;
DEFINE FIELD IF NOT EXISTS userId ON TABLE message TYPE string;

DEFINE TABLE IF NOT EXISTS assignment SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE assignment TYPE string;
DEFINE FIELD IF NOT EXISTS due ON TABLE assignment TYPE option<datetime>;
DEFINE FIELD IF NOT EXISTS classId ON TABLE assignment TYPE string;
DEFINE FIELD IF NOT EXISTS text ON TABLE assignment TYPE string;

DEFINE TABLE IF NOT EXISTS submission SCHEMALESS;
DEFINE FIELD IF NOT EXISTS date ON TABLE submission TYPE datetime DEFAULT time::now();
DEFINE FIELD IF NOT EXISTS assignmentId ON TABLE submission TYPE string;
DEFINE FIELD IF NOT EXISTS text ON TABLE submission TYPE string;
DEFINE FIELD IF NOT EXISTS userId ON TABLE submission TYPE string;

DEFINE TABLE IF NOT EXISTS quiz SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE quiz TYPE string;
DEFINE FIELD IF NOT EXISTS description ON TABLE quiz TYPE string;
DEFINE FIELD IF NOT EXISTS userId ON TABLE quiz TYPE string;
DEFINE FIELD IF NOT EXISTS code ON TABLE quiz TYPE string;

DEFINE ANALYZER quiz_analyzer TOKENIZERS class, blank FILTERS lowercase, ascii;
DEFINE INDEX name_index ON TABLE quiz COLUMNS name SEARCH ANALYZER quiz_analyzer BM25;
DEFINE INDEX desc_index ON TABLE quiz COLUMNS description SEARCH ANALYZER quiz_analyzer BM25;
DEFINE INDEX code_index ON TABLE quiz COLUMNS code SEARCH ANALYZER quiz_analyzer BM25;
""");
}

