using System.Net;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IUserService
{ 
     Task<HttpStatusCode> CreateUser(User user); 
     Task<HttpStatusCode> DeleteUser(string userId); 
     Task<bool> CheckUserExists(string userId);
}