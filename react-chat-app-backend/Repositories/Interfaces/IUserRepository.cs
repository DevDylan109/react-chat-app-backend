using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories.Interfaces;

public interface IUserRepository
{
     Task<User?> GetUser(string userId);
     Task CreateUser(User user);
     Task RemoveUser(User user);
     Task SetUsername(string userId, string newName);
}