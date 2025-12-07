using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using studbud.Shared;
using studbud.Shared.Models;

namespace studbud.Client.Shared;

public class HubService {
    public IHubConnectionBuilder GetHubConnection()
    {
        return new HubConnectionBuilder().WithAutomaticReconnect().WithUrl("http://localhost:5001/apphub").AddJsonProtocol(cfg =>
        {
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            cfg.PayloadSerializerOptions = jsonOptions;
        });
    }

    public async Task<User?> AutoLogin(HubConnection hub, Blazored.SessionStorage.ISessionStorageService sessionStorage)
    {
        var user = await sessionStorage.GetItemAsync<User>("user");
        if (user is not null)
        {
            var check = await hub.InvokeAsync<User?>(nameof(IAppHubServer.CheckUser), user.id);
            await sessionStorage.SetItemAsync("user", check);
            return check;
        }
        else
        {
            return null;
        }
    }
}