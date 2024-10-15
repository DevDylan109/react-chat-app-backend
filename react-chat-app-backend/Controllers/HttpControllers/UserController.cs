using System.Net;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Models;
using react_chat_app_backend.Services;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Controllers.HttpControllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet("GetUser/{userId}")]
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
    
    [HttpPost("LoginUser/{userId}/{password}")]
    public async Task<IActionResult> LoginUser(string userId, string password)
    {
        var user = await _userService.GetUser(userId);
        return user.password == password ? Ok() : BadRequest(); 
    }
    
    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser(User user)
    {
        var result = await _userService.CreateUser(user);

        return result switch
        {
            HttpStatusCode.Conflict => Conflict(),
            HttpStatusCode.Created => CreatedAtAction(nameof(CreateUser), user)
        };
    }
    
    [HttpDelete("DeleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _userService.DeleteUser(userId);

        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }

    [HttpPut("ChangeUsername/{userId}/{username}")]
    public async Task<IActionResult> ChangeUsername(string userId, string username)
    {
        var result = await _userService.ChangeUserName(userId, username);

        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }
    
    [HttpPut("ChangeProfilePicture/{userId}/{imageURL}")]
    public async Task<IActionResult> ChangeProfilePicture(string userId, string imageURL)
    {
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

}