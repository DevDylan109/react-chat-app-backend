using System.Net;
using react_chat_app_backend.DTOs;
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

    public async Task<User> GetUser(string userId)
    {
        return await _userRepository.GetUser(userId);
    }

    public async Task<HttpStatusCode> CreateUser(UserRegistrationDTO user)
    {
        if (await CheckUserExists(user.Username)) {
            return HttpStatusCode.Conflict;
        }

        var newUser = new User(user.Username, user.Password, user.DisplayName);
        await _userRepository.CreateUser(newUser);
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

    public async Task<HttpStatusCode> CheckUsernameExists(string username)
    {
        if (await _userRepository.CheckUsernameExists(username))
        {
            return HttpStatusCode.OK;
        }
        else
        {
            return HttpStatusCode.NotFound;
        }
    }

    public async Task<HttpStatusCode> ChangeUserName(string userId, string newUsername)
    {
        if (await CheckUserExists(userId) == false)
        {
            return HttpStatusCode.NotFound;
        }
        
        await _userRepository.SetUsername(userId, newUsername);
        return HttpStatusCode.OK;
    }

    public async Task<HttpStatusCode> ChangeProfilePicture(string userId, string imageURL)
    {
        if (await CheckUserExists(userId) == false)
        {
            return HttpStatusCode.NotFound;
        }

        await _userRepository.SetImageURL(userId, imageURL);
        return HttpStatusCode.OK;
    }

}