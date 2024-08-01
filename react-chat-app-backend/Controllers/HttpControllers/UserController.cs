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

    [HttpPost]
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
    
    [HttpDelete("{userId}")]
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
    
}