using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using react_chat_app_backend.Controllers.WSControllers;
using react_chat_app_backend.DTOs;
using react_chat_app_backend.Models;
using react_chat_app_backend.Results.Enums;
using react_chat_app_backend.Services;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Controllers.HttpControllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpGet("GetUser/{UserId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _userService.GetUser(userId);
        user.password = "";
        
        return user switch
        {
            null => NotFound("There is no user with this userID."),
            not null => Ok(user)
        };
    }
    
    // [EnableRateLimiting("fixed")]
    [HttpPost("LoginUser/{UserId}/{password}")]
    public async Task<IActionResult> LoginUser(string userId, string password)
    {
        var user = await _userService.GetUser(userId);
        var token = _tokenService.CreateAndStore(userId, 120);
        return user.password == password ? Ok(token) : BadRequest("invalid password"); 
    }
    
    [HttpPost("CreateUser/{username}/{displayname}/{password}")]
    public async Task<IActionResult> CreateUser(string username, string displayname, string password)
    {
        var result = await _userService.CreateUser(username, displayname, password);
        var token = _tokenService.CreateAndStore(username, 120);
        
        return result.UserOutcome switch
        {
            UserOutcome.InputInvalid => BadRequest(result.Message),
            UserOutcome.UserAlreadyExists => Conflict(result.Message),
            UserOutcome.UserCreated => Ok(token)
        };
    }
    
    [HttpDelete("DeleteUser/{UserId}/{token}")]
    public async Task<IActionResult> DeleteUser(string userId, string token)
    {
        if (_tokenService.IsTokenValid(userId, token) == false) {
            return BadRequest("Token not valid");
        }
        
        var result = await _userService.DeleteUser(userId);
        return result.UserOutcome switch
        {
            UserOutcome.UserNotFound => NotFound(result.Message),
            UserOutcome.UserDeleted => Ok(result.Message)
        };
    }

    [HttpPut("ChangeUsername/{UserId}/{username}/{token}")]
    public async Task<IActionResult> ChangeUsername(string userId, string username, string token)
    {
        if (_tokenService.IsTokenValid(userId, token) == false) {
            return BadRequest("Token not valid");
        }
        
        var result = await _userService.ChangeUserName(userId, username);
        return result.UserOutcome switch
        {
            UserOutcome.UserNotFound => NotFound(result.Message),
            UserOutcome.ChangedUsername => Ok(result.Message)
        };
    }
    
    [HttpPut("ChangeProfilePicture")]
    public async Task<IActionResult> ChangeProfilePicture(ProfilePicture picture)
    {
        if (_tokenService.IsTokenValid(picture.UserId, picture.Token) == false) {
            return BadRequest("Token not valid");
        }
        
        var result = await _userService.ChangeProfilePicture(picture.UserId, picture.ImageURL);
        return result.UserOutcome switch
        {
            UserOutcome.UserNotFound => NotFound(result.Message),
            UserOutcome.ChangedProfilePicture => Ok(result.Message)
        };
    }
    
    [HttpPost("CheckUsernameExists/{username}")]
    public async Task<IActionResult> CheckUsernameExists(string username)
    {
        var result = await _userService.CheckUsernameExists(username);
        return result switch
        {
            false => NotFound("This username does not exist."),
            true => Ok("This username exists.")
        };
    }
    
    [HttpGet("FetchStatus/{UserId}")]
    public async Task<IActionResult> FetchStatus(string userId)
    {
        var result = await _userService.GetOnlineStatus(userId);
        
        return result switch
        {
            true => Ok("This user is online."),
            false => NotFound("This user is offline.")
        };
    }

}