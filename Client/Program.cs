using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;
using studbud.Client;
using studbud.Client.Shared;
using studbud.Shared.Models;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddSingleton<HubService>();
await builder.Build().RunAsync();
