using System.Collections.Generic;
using System.Threading.Tasks;
using studbud.Shared.Models;

namespace studbud.Shared;

public interface IAppHubServer
{
    Task<Message> SendMessage(Message msg);
    Task<Chat> CreateChat(Chat chat);
    Task<Class> CreateClass(Class clss);
    Task<Class?> JoinClass(string userId, string code);
    Task ConnectToChat(string parent);
    Task<List<Message>> GetMessages(string parent);
    Task<User?> GetUserFromUsername(string username);
    Task<Chat> GetChat(string id);
    Task<Class> GetClass(string id);
    Task<Assignment> CreateAssignment(Assignment ass);
    Task<List<Assignment>> GetAssignments(string classId);
    Task<Submission> SubmitAssignment(Submission sub);
    Task<List<Submission>> GetSubmissions(string assignmentId);
    Task<List<Class>> GetClassesFromUser(string id);
    Task<List<Chat>> GetChatsFromUser(string id);
    Task<Chat> GetChatWithName(string chatId, string userId);
    Task<User> GetUser(string id);
    Task<User?> CheckUser(string id);
    Task<List<User>> GetUsers(List<string> ids);
    Task<string> GetChatName(Chat chat, string userId);
    Task<User?> SignIn(string username, string password);
    Task<User?> SignUp(UserInfo user);
}