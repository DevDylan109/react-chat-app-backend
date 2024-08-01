using System.Net;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories;

public interface IUserRepository
{
     Task<UserData?> GetUser(string userId);
     Task CreateUser(UserData userData);
     Task RemoveUser(UserData user);
}