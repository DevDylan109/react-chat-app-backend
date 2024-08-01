using System.Net;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Services;

public class UserService : IUserService
{
    private IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<HttpStatusCode> CreateUser(User user)
    {
        var userId = user.userId;
        
        if (await CheckUserExists(userId)) {
            return HttpStatusCode.Conflict;
        }

        await _userRepository.CreateUser(user);
        return HttpStatusCode.Created;
    }
    
    public async Task<HttpStatusCode> DeleteUser(string userId)
    {
        var user = await _userRepository.GetUser(userId);

        if (user == null)
            return HttpStatusCode.NotFound;

        await _userRepository.RemoveUser(user);
        return HttpStatusCode.OK;
    }
    
    public async Task<bool> CheckUserExists(string userId)
    {
        var user = await _userRepository.GetUser(userId);
        return user != null;
    }
    
}