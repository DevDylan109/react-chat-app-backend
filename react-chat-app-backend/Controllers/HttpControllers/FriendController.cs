using System.Net;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Models;
using react_chat_app_backend.Services;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Controllers.HttpControllers;

[ApiController]
[Route("api/[controller]")]
public class FriendController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IFriendShipService _friendShipService;
    
    public FriendController(ILogger<UserController> logger, IFriendShipService friendShipService)
    {
        _logger = logger;
        _friendShipService = friendShipService;
    }

    [HttpPost("SendFriendRequest")]
    public async Task<IActionResult> StoreAndForwardFriendRequest(FriendRequest friendRequest)
    {
        var result = await _friendShipService.StoreAndForwardFriendRequest(friendRequest);

        return result switch
        {
            HttpStatusCode.BadRequest => BadRequest(),
            HttpStatusCode.NotFound => NotFound(),
            HttpStatusCode.Conflict => Conflict(),
            HttpStatusCode.Created => Created()
        };
    }
    
    [HttpPut("AcceptFriendRequest")]
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
    
    [HttpGet("FetchFriends/{userId}")]
    public async Task<IActionResult> FetchFriends(string userId)
    {
        var result = await _friendShipService.GetFriendsOfUser(userId);
        
        return result switch
        {
            not null => Ok(result),
            null => NotFound()
        };
    }

    [HttpGet("FetchIncomingFriendRequests/{userId}")]
    public async Task<IActionResult> FetchIncomingFriendRequests(string userId)
    {
        var result = await _friendShipService.GetIncomingFriendRequestsOfUser(userId);

        return result switch
        {
            not null => Ok(result),
            null => NotFound()
        };
    }
    
    [HttpGet("FetchOutgoingFriendRequests/{userId}")]
    public async Task<IActionResult> FetchOutgoingFriendRequests(string userId)
    {
        var result = await _friendShipService.GetOutgoingFriendRequestsOfUser(userId);

        return result switch
        {
            not null => Ok(result),
            null => NotFound()
        };
    }

}