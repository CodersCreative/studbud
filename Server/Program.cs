using System.Diagnostics;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor;
using OllamaSharp;
using studbud.Hubs;
using SurrealDb.Net;

var builder = WebApplication.CreateBuilder(args);
var surrealUrl = "http://127.0.0.1:8000";

var surreal = SurrealDbOptions
    .Create()
    .WithEndpoint(surrealUrl)
    .WithNamespace("main")
    .WithDatabase("main")
    .Build();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});
builder.Services.AddSurreal(surreal);

string? origins = "origins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        origins,
        policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        }
    );
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
    HttpClient httpClient = new HttpClient();

    var needsStart = false;

    try
    {
        needsStart = !(await httpClient.GetAsync(surrealUrl)).IsSuccessStatusCode;
    }catch (Exception e)
    {
        needsStart = true;
    }

    if (needsStart) {
        Thread newThread = new Thread(new ThreadStart(() => Process.Start("surreal start memory --allow-all --unauthenticated")));
        newThread.Start();
        Thread.Sleep(500);
    }

    var surrealDbClient = new SurrealDbClient(surreal);
    await DefineSchemaAsync(surrealDbClient);
}

async Task DefineSchemaAsync(ISurrealDbClient surrealDbClient)
{
    await surrealDbClient.RawQuery(
        """
DEFINE TABLE IF NOT EXISTS user SCHEMALESS;
DEFINE FIELD IF NOT EXISTS username ON TABLE user TYPE string;
DEFINE FIELD IF NOT EXISTS money ON TABLE user TYPE float;
DEFINE FIELD IF NOT EXISTS email ON TABLE user TYPE string;
DEFINE FIELD IF NOT EXISTS password ON TABLE user TYPE string;

DEFINE TABLE IF NOT EXISTS class SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE class TYPE string;
DEFINE FIELD IF NOT EXISTS code ON TABLE class TYPE string;
DEFINE FIELD IF NOT EXISTS userIds ON TABLE class TYPE array<string>;
DEFINE FIELD IF NOT EXISTS teacherIds ON TABLE class TYPE array<string>;

DEFINE TABLE IF NOT EXISTS chat SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE chat TYPE option<string>;
DEFINE FIELD IF NOT EXISTS userIds ON TABLE chat TYPE array<string>;

DEFINE ANALYZER chat_analyzer TOKENIZERS class, blank FILTERS lowercase, ascii;
DEFINE INDEX name_index ON TABLE chat COLUMNS name SEARCH ANALYZER chat_analyzer BM25;

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
DEFINE FIELD IF NOT EXISTS cost ON TABLE quiz TYPE float DEFAULT 0.0;
DEFINE FIELD IF NOT EXISTS published ON TABLE quiz TYPE bool DEFAULT false;
DEFINE FIELD IF NOT EXISTS description ON TABLE quiz TYPE string;
DEFINE FIELD IF NOT EXISTS userId ON TABLE quiz TYPE string;
DEFINE FIELD IF NOT EXISTS code ON TABLE quiz TYPE string;

DEFINE ANALYZER quiz_analyzer TOKENIZERS class, blank FILTERS lowercase, ascii;
DEFINE INDEX name_index ON TABLE quiz COLUMNS name SEARCH ANALYZER quiz_analyzer BM25;
DEFINE INDEX desc_index ON TABLE quiz COLUMNS description SEARCH ANALYZER quiz_analyzer BM25;
DEFINE INDEX code_index ON TABLE quiz COLUMNS code SEARCH ANALYZER quiz_analyzer BM25;

DEFINE TABLE IF NOT EXISTS flashcard SCHEMALESS;
DEFINE FIELD IF NOT EXISTS name ON TABLE flashcard TYPE string;
DEFINE FIELD IF NOT EXISTS cost ON TABLE flashcard TYPE float DEFAULT 0.0;
DEFINE FIELD IF NOT EXISTS published ON TABLE flashcard TYPE bool DEFAULT false;
DEFINE FIELD IF NOT EXISTS description ON TABLE flashcard TYPE string;
DEFINE FIELD IF NOT EXISTS userId ON TABLE flashcard TYPE string;
DEFINE FIELD IF NOT EXISTS code ON TABLE flashcard TYPE string;

DEFINE TABLE IF NOT EXISTS flashcard_card SCHEMALESS;
DEFINE FIELD IF NOT EXISTS front ON TABLE flashcard_card TYPE string;
DEFINE FIELD IF NOT EXISTS back ON TABLE flashcard_card TYPE string;
DEFINE FIELD IF NOT EXISTS flashcardId ON TABLE flashcard_card TYPE string;

DEFINE ANALYZER flashcard_analyzer TOKENIZERS class, blank FILTERS lowercase, ascii;
DEFINE INDEX name_index ON TABLE flashcard COLUMNS name SEARCH ANALYZER flashcard_analyzer BM25;
DEFINE INDEX desc_index ON TABLE flashcard COLUMNS description SEARCH ANALYZER flashcard_analyzer BM25;
DEFINE INDEX code_index ON TABLE flashcard COLUMNS code SEARCH ANALYZER flashcard_analyzer BM25;

DEFINE TABLE IF NOT EXISTS payment SCHEMALESS;
DEFINE FIELD IF NOT EXISTS fromId ON TABLE payment TYPE string;
DEFINE FIELD IF NOT EXISTS toId ON TABLE payment TYPE string;
DEFINE FIELD IF NOT EXISTS amount ON TABLE payment TYPE float;
DEFINE FIELD IF NOT EXISTS reason ON TABLE payment TYPE string;
DEFINE FIELD IF NOT EXISTS reasonType ON TABLE payment TYPE string;
"""
    );
}
