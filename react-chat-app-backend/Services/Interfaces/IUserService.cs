using System.Net;
using react_chat_app_backend.Models;
using react_chat_app_backend.Results;

namespace react_chat_app_backend.Services.Interfaces;

public interface IUserService
{
     Task<User> GetUser(string userId);
     Task<bool> GetOnlineStatus(string userId);
     Task<UserResult> CreateUser(string username, string displayname, string password); 
     Task<UserResult> DeleteUser(string userId); 
     Task<bool> CheckUserExists(string userId);
     Task<UserResult> ChangeUserName(string userId, string newUsername);
     Task<UserResult> ChangeProfilePicture(string userId, string imageURL);
     Task<bool> CheckUsernameExists(string username);
}