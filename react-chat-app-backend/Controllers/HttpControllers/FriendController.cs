using System.Net;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Models;
using react_chat_app_backend.Results.Enums;
using react_chat_app_backend.Services;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Controllers.HttpControllers;

[ApiController]
[Route("api/[controller]")]
public class FriendController : ControllerBase
{
    private readonly IFriendShipService _friendShipService;
    private readonly ITokenService _tokenService;
    
    public FriendController(IFriendShipService friendShipService, ITokenService tokenService)
    {
        _friendShipService = friendShipService;
        _tokenService = tokenService;
    }

    [HttpPost("SendFriendRequest/{initiatorId}/{acceptorId}/{token}")]
    public async Task<IActionResult> StoreAndForwardFriendRequest(string initiatorId, string acceptorId, string token)
    {
        if (_tokenService.IsTokenValid(initiatorId, token) == false) {
            return BadRequest("invalid token");
        }
        
        var result = await _friendShipService
            .StoreAndForwardFriendRequest(initiatorId, acceptorId);
        
        return result.Outcome switch
        {
            FriendShipOutcome.UserAddSelf => BadRequest(result.Message),
            FriendShipOutcome.UserNotFound => NotFound(result.Message),
            FriendShipOutcome.FriendShipAlreadyAccepted => Conflict(result.Message),
            FriendShipOutcome.FriendShipIsPending => Conflict(result.Message),
            FriendShipOutcome.FriendShipCreated => Created()
        };
    }
    
    [HttpPut("AcceptFriendRequest/{initiatorId}/{acceptorId}/{token}")]
    public async Task<IActionResult> AcceptFriendRequest(string initiatorId, string acceptorId, string token)
    {
        if (_tokenService.IsTokenValid(initiatorId, token) == false) {
            if (_tokenService.IsTokenValid(acceptorId, token) == false) {
                return BadRequest("invalid token");   
            }
        }
        
        var result = await _friendShipService
            .AcceptFriendRequest(initiatorId, acceptorId);
        
        return result.Outcome switch
        {
            FriendShipOutcome.FriendShipAccepted => Ok(result.Message),
            FriendShipOutcome.FriendShipAlreadyAccepted => Conflict(result.Message),
            FriendShipOutcome.FriendShipDoesNotExist => NotFound(result.Message)
        };
    }

    [HttpDelete("DeclineFriendRequest/{initiatorId}/{acceptorId}/{token}")]
    public async Task<IActionResult> DeclineFriendRequest(string initiatorId, string acceptorId, string token)
    {
        if (_tokenService.IsTokenValid(initiatorId, token) == false) {
            if (_tokenService.IsTokenValid(acceptorId, token) == false) {
                return BadRequest("invalid token");   
            }
        }
        
        var result = await _friendShipService
            .DeclineFriendRequest(initiatorId, acceptorId);
        
        return result.Outcome switch
        {
             FriendShipOutcome.FriendShipDeclined => Ok(),
             FriendShipOutcome.FriendShipAlreadyAccepted => Conflict(),
             FriendShipOutcome.FriendShipDoesNotExist => NotFound()
        };
    }
    
    [HttpDelete("RemoveFriend/{userId1}/{userId2}/{token}")]
    public async Task<IActionResult> RemoveFriendOfUser(string userId1, string userId2, string token)
    {
        if (_tokenService.IsTokenValid(userId1, token) == false) {
            return BadRequest("invalid token");
        }
        
        var result = await _friendShipService.RemoveFriend(userId1, userId2);
        return result.Outcome switch
        {
            FriendShipOutcome.FriendShipDeleted => Ok(),
            FriendShipOutcome.UserNotFound => NotFound()
        };
    }
    
    [HttpGet("FetchFriends/{UserId}")]
    public async Task<IActionResult> FetchFriends(string userId)
    {
        var result = await _friendShipService.GetFriendsOfUser(userId);
        return result switch
        {
            not null => Ok(result),
            null => NotFound()
        };
    }

    [HttpGet("FetchIncomingFriendRequests/{UserId}")]
    public async Task<IActionResult> FetchIncomingFriendRequests(string userId)
    {
        var result = await _friendShipService.GetIncomingFriendRequestsOfUser(userId);
        return result switch
        {
            not null => Ok(result),
            null => NotFound()
        };
    }
    
    [HttpGet("FetchOutgoingFriendRequests/{UserId}")]
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