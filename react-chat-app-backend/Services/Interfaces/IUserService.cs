using System.Net;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IUserService
{
     Task<User> GetUser(string userId);
     Task<bool> GetOnlineStatus(string userId);
     Task<HttpStatusCode> CreateUser(string username, string displayname, string password); 
     Task<HttpStatusCode> DeleteUser(string userId); 
     Task<bool> CheckUserExists(string userId);
     Task<HttpStatusCode> ChangeUserName(string userId, string newUsername);
     Task<HttpStatusCode> ChangeProfilePicture(string userId, string imageURL);
     Task<HttpStatusCode> CheckUsernameExists(string username);
}