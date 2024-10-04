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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(string userId)
    {
        var result = await _userService.GetUser(userId);
        
        return result switch
        {
            null => NotFound(),
            not null => Ok(result)
        };
    }
    
    [HttpPost("LoginUser/{userId}/{password}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LoginUser(string userId, string password)
    {
        var user = await _userService.GetUser(userId);
        return user.password == password ? Ok() : BadRequest(); 
    }
    
    [HttpPost("CreateUser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeUsername(string userId, string username)
    {
        var result = await _userService.ChangeUserName(userId, username);

        return result switch
        {
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.OK => Ok()
        };
    }
    
    [HttpPost("CheckUsernameExists/{username}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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