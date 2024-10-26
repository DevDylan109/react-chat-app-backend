using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using react_chat_app_backend.Controllers.WSControllers;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;
using react_chat_app_backend.Results;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Services;

public class UserService : IUserService
{
    private IUserRepository _userRepository;
    private IWSManager _wsManager;
    private IWSMessageService _wsMessageService;
    
    public UserService(IUserRepository userRepository, IWSManager wsManager, IWSMessageService wsMessageService)
    {
        _userRepository = userRepository;
        _wsManager = wsManager;
        _wsMessageService = wsMessageService;
    }

    public async Task<User> GetUser(string userId) =>
        await _userRepository.GetUser(userId);
    
    public async Task<bool> CheckUsernameExists(string username) => 
        await _userRepository.CheckUsernameExists(username);
    
    public async Task<bool> CheckUserExists(string userId) => 
        await _userRepository.GetUser(userId) != null;
    
    public async Task<UserResult> CreateUser(string username, string displayname, string password)
    {
        if (await CheckUserExists(username)) {
            // return HttpStatusCode.Conflict;
            return UserResult.UserAlreadyExists();
        }

        if (isDisplayNameValid(displayname) == false ||
            isUsernameValid(username) == false ||
            isPasswordValid(password) == false)
        {
            // return HttpStatusCode.BadRequest;
            return UserResult.InputInvalid();
        }
        
        var newUser = new User(username, password, displayname);
        await _userRepository.CreateUser(newUser);
        // return HttpStatusCode.Created;
        return UserResult.UserCreated();
    }
    
    public async Task<UserResult> DeleteUser(string userId)
    {
        var user = await _userRepository.GetUser(userId);

        if (user == null)
            // return HttpStatusCode.NotFound;
            return UserResult.UserNotFound();

        await _userRepository.RemoveUser(user);
        // return HttpStatusCode.OK;
        return UserResult.UserDeleted();
    }

    public async Task<UserResult> ChangeUserName(string userId, string newUsername)
    {
        if (await CheckUserExists(userId) == false) {
            // return HttpStatusCode.NotFound;
            return UserResult.UserNotFound();
        }
        
        await _userRepository.SetUsername(userId, newUsername);
        await _wsMessageService.BroadcastMessage(userId, 
            new { userId, newUsername, type = "changedUsername" }
        );
        
        // return HttpStatusCode.OK;
        return UserResult.ChangedUsername();
    }

    public async Task<UserResult> ChangeProfilePicture(string userId, string imageURL)
    {
        if (await CheckUserExists(userId) == false) {
            // return HttpStatusCode.NotFound;
            return UserResult.UserNotFound();
        }

        await _userRepository.SetImageURL(userId, imageURL);
        await _wsMessageService.BroadcastMessage(userId, new { userId, imageURL, type = "changedProfilePicture" });
        // return HttpStatusCode.OK;
        return UserResult.ChangedProfilePicture();
    }
    
    public async Task<bool> GetOnlineStatus(string userId)
    {
        // user can be online on multiple devices
        var isOnline = _wsManager.All()
            .Where(wsClient => wsClient.userId == userId)
            .Any(wsClient => wsClient.webSocket.State == WebSocketState.Open);

        return isOnline;
    }

    public bool isDisplayNameValid(string diplayName)
    {
        if (string.IsNullOrEmpty(diplayName)) 
            return false;
        if (diplayName.Length < 6 || diplayName.Length > 30) 
            return false;
        if (!Regex.IsMatch(diplayName, @"^[a-zA-Z0-9 ]*$")) 
            return false;

        return true;
    }
    
    public bool isUsernameValid(string username)
    {
        if (string.IsNullOrEmpty(username)) 
            return false;
        if (username.Length < 6 || username.Length > 30) 
            return false;
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9 ]*$")) 
            return false;

        return true;
    }
    
    public bool isPasswordValid(string password)
    {
        if (string.IsNullOrEmpty(password)) {
            return false;
        }
        if (password.Length < 12 || password.Length > 100) {
            return false;
        }
        if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$")) {
            return false;
        }

        return true;
    }

}