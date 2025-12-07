using Microsoft.AspNetCore.SignalR;
using SurrealDb.Net;
using studbud.Shared.Models;
using studbud.Shared;
using System.Reactive.Threading.Tasks;
using System.Text.Json;

namespace studbud.Hubs;

public class AppHub : Hub<IAppHubClient>, IAppHubServer
{
    private Dictionary<String, String> connectedChats = new();
    private readonly SurrealDbClient dbClient;

    public AppHub (SurrealDbClient dbClient)
    {
        this.dbClient = dbClient;
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine("Connected");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Disconnected");
        Console.WriteLine(exception);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task<Message> SendMessage(Message msg)
    {
        var res = await dbClient.Create("message", new DbMessage(msg));
        var message = res.ToBase();
        //await Clients.Clients(connectedChats.Where((x) => x.Value == msg.parentId && x.Key != Context.ConnectionId).Select((x) => x.Key)).ReceiveMessage(message);
        await Clients.AllExcept(Context.ConnectionId).ReceiveMessage(message);
        return message;
    }

    public async Task<Chat> CreateChat(Chat chat)
    {
        var res = await dbClient.Create("chat", new DbChat(chat));
        return res.ToBase();
    }

    public async Task<Class> CreateClass(Class clss)
    {
        clss.code ??= new Random().Next(99999).ToString();
        
        var res = await dbClient.Create("class", new DbClass(clss));
        return res.ToBase();
    }

    public async Task<Class?> JoinClass(string userId, string code)
    {
        var result = await dbClient.Query($"UPDATE class WHERE code = {code} SET userIds += {userId};");        
        return result?.GetValue<List<DbClass>>(0)?.First()?.ToBase();
    }

    public async Task ConnectToChat(string parent)
    {
        connectedChats.Add(Context.ConnectionId, parent);
    }

    public async Task<List<Message>> GetMessages(string parent)
    {
        var result = await dbClient.Query($"SELECT * FROM message WHERE parentId = {parent} ORDER BY Date ASC;");
        var messages_res = result.GetValue<List<DbMessage>>(0);
        if (messages_res is not null) {
            return messages_res.Select((x) => x.ToBase()).ToList();
        } else
        {
            return [];
        }
    }

    public async Task<User?> GetUserFromUsername(string username)
    {
        var res = await dbClient.Query($"SELECT * FROM user WHERE username = {username} LIMIT 1;");
        return res.GetValue<List<DbUser>>(0)?.First()?.ToBase();
    }

    public async Task<Chat> GetChat(string id)
    {
        var res = await dbClient.Select<DbChat>(("chat", id));
        return res!.ToBase();
    }

    public async Task<List<Class>> GetClassesFromUser(string id)
    {
        var result = await dbClient.Query($"SELECT * FROM class WHERE userIds CONTAINS {id} OR teacherIds CONTAINS {id};");
        var classes = result.GetValue<List<DbClass>>(0);
        if (classes is not null) {
            return classes.Select((x) => x.ToBase()).ToList();
        } else
        {
            return [];
        }
    }

    public async Task<List<Chat>> GetChatsFromUser(string id)
    {
        var result = await dbClient.Query($"SELECT * FROM chat WHERE userIds CONTAINS {id};");
        var chats = result.GetValue<List<DbChat>>(0);
        if (chats is not null) {
            var chts = chats.Select((x) => x.ToBase()).ToList();
            for (int i = 0; i < chts.Count; i++)
            {
                chts[i].name = await GetChatNameInternal(chts[i], id);
            }
            return chts;
        }else
        {
            return [];
        }
    }

    public async Task<Chat> GetChatWithName(string chatId, string userId)
    {
        var chat = (await dbClient.Select<DbChat>(("chat", chatId)))!.ToBase();
        chat.name = await GetChatNameInternal(chat, userId);
        return chat;
    }

    public async Task<User> GetUser(string id)
    {
        var res = await dbClient.Select<DbUser>(("user", id));
        return res!.ToBase();
    }

        public async Task<User?> CheckUser(string id)
    {
        var res = await dbClient.Select<DbUser>(("user", id));
        return res?.ToBase();
    }

    public async Task<List<User>> GetUsers(List<string> ids)
    {
        List<User> users = [];
        foreach (var id in ids) {
            users.Add(
                (await dbClient.Select<DbUser>(("user", id)))!.ToBase()
            );
        }
        return users;
    }

    public async Task<string> GetChatNameInternal(Chat chat, string userId)
    {
        if (chat.name is null) {
            var other = chat.userIds.First();
            if (other == userId) {
                other = chat.userIds[1];
            }

            var res = await dbClient.Select<DbUser>(("user", other));
            if (res is not null) {
                return res.username;
            }else
            {
                return other;
            }
        }else
        {
            return chat.name;
        }
    }

    public async Task<string> GetChatName(Chat chat, string userId)
    {
        return await GetChatNameInternal(chat, userId);
    }

    public async Task<User?> SignIn(string username, string password)
    {
        var res = await dbClient.Query($"SELECT * FROM user WHERE username = {username} and password = {password} LIMIT 1;");

        var res2 = res.GetValue<List<DbUser>>(0);

        if (res2 is not null) {
            if (res2.Count > 0) {
                return res2.First().ToBase();
            }
        }

        return null;
    }

    public async Task<User?> SignUp(UserInfo user)
    {
        var res = await dbClient.Query($"SELECT * FROM user WHERE username = {user.username} and password = {user.password} LIMIT 1;");

        var res2 = res.GetValue<List<DbUser>>(0);

        if (res2 is not null) {
            if (res2.Count > 0) {
                return res2.First().ToBase();
            }
        }

        res = await dbClient.Query($"SELECT * FROM user WHERE username = {user.username};");
        res2 = res.GetValue<List<DbUser>>(0);

        if (res2 is not null) {
            if (res2.Count > 0){
                return null;
            }
        }

        var usr = await dbClient.Create("user", new DbUserInfo(user));
        return usr.ToBase().ToBase();
    }
}