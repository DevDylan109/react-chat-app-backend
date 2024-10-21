using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using react_chat_app_backend.Models;
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
            null => NotFound(),
            not null => Ok(user)
        };
    }
    
    // [EnableRateLimiting("fixed")]
    [HttpPost("LoginUser/{UserId}/{password}")]
    public async Task<IActionResult> LoginUser(string userId, string password)
    {
        var user = await _userService.GetUser(userId);
        var token = _tokenService.CreateAndStore(userId, 120);
        return user.password == password ? Ok(token) : BadRequest(); 
    }
    
    [HttpPost("CreateUser/{username}/{displayname}/{password}")]
    public async Task<IActionResult> CreateUser(string username, string displayname, string password)
    {
        var result = await _userService.CreateUser(username, displayname, password);
        var token = _tokenService.CreateAndStore(username, 120);
        
        return result switch
        {
            HttpStatusCode.BadRequest => BadRequest(),
            HttpStatusCode.Conflict => Conflict(),
            HttpStatusCode.Created => Ok(token)
        };
    }
    
    [HttpDelete("DeleteUser/{UserId}/{token}")]
    public async Task<IActionResult> DeleteUser(string userId, string token)
    {
        if (_tokenService.IsTokenValid(userId, token) == false) {
            return BadRequest("Token not valid");
        }
        
        var result = await _userService.DeleteUser(userId);
        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }

    [HttpPut("ChangeUsername/{UserId}/{username}/{token}")]
    public async Task<IActionResult> ChangeUsername(string userId, string username, string token)
    {
        if (_tokenService.IsTokenValid(userId, token) == false) {
            return BadRequest("Token not valid");
        }
        
        var result = await _userService.ChangeUserName(userId, username);
        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }
    
    [HttpPut("ChangeProfilePicture/{UserId}/{imageURL}/{token}")]
    public async Task<IActionResult> ChangeProfilePicture(string userId, string imageURL, string token)
    {
        if (_tokenService.IsTokenValid(userId, token) == false) {
            return BadRequest("Token not valid");
        }
        
        imageURL = Uri.UnescapeDataString(imageURL);
        
        var result = await _userService.ChangeProfilePicture(userId, imageURL);
        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }
    
    [HttpPost("CheckUsernameExists/{username}")]
    public async Task<IActionResult> CheckUsernameExists(string username)
    {
        var result = await _userService.CheckUsernameExists(username);
        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }
    
    [HttpGet("FetchStatus/{UserId}")]
    public async Task<IActionResult> FetchStatus(string userId)
    {
        var result = await _userService.GetOnlineStatus(userId);
        return result switch
        {
            HttpStatusCode.OK => Ok(),
            HttpStatusCode.NotFound => NotFound()
        };
    }

}