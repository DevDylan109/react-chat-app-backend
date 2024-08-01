using System.Net;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Models;
using react_chat_app_backend.Services;

namespace react_chat_app_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly FriendShipService _friendShipService;

    public FriendController(ILogger<UserController> logger, FriendShipService friendShipService)
    {
        _logger = logger;
        _friendShipService = friendShipService;
    }

    [HttpPost("SendFriendRequest")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StoreAndForwardFriendRequest(FriendRequest friendRequest)
    {
        var result = await _friendShipService.StoreAndForwardFriendRequest(friendRequest);

        return result switch
        {
            HttpStatusCode.Conflict => Conflict(),
            HttpStatusCode.Created => Created()
        };
    }
    
    [HttpPut("AcceptFriendRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptFriendRequest(FriendRequest friendRequest)
    {
        var result = await _friendShipService.AcceptFriendRequest(friendRequest);

        return result switch
        {
            HttpStatusCode.OK => Ok(),
            HttpStatusCode.Conflict => Conflict(),
            HttpStatusCode.NotFound => NotFound()
        };
    }

    [HttpDelete("DeclineFriendRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeclineFriendRequest(FriendRequest friendRequest)
    {
        var result = await _friendShipService.DeclineFriendRequest(friendRequest);
        
        return result switch
        {
             HttpStatusCode.OK => Ok(),
             HttpStatusCode.Conflict => Conflict(),
             HttpStatusCode.NotFound => NotFound()
        };
    }
    
    [HttpDelete("RemoveFriend/{userId1}/{userId2}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFriendOfUser(string userId1, string userId2)
    {
        var result = await _friendShipService.RemoveFriend(userId1, userId2);
        
        return result switch
        {
            HttpStatusCode.OK => Ok(),
            HttpStatusCode.Conflict => Conflict(),
            HttpStatusCode.NotFound => NotFound()
        };
    }
    
    [HttpDelete("FetchFriends/{userId1}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FetchFriends(string userId1)
    {
        var result = await _friendShipService.GetFriendsOfUser(userId1);
        
        return result switch
        {
            not null => Ok(),
            null => NotFound()
        };
    }
    
}