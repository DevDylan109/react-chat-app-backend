using System.Net;
using System.Text.Json;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Services;

public class FriendShipService : IFriendShipService
{
    private IFriendShipRepository _friendShipRepository;
    private IWSMessageService _wsMessageService;
    private IUserService _userService;
    
    public FriendShipService(IFriendShipRepository friendShipRepository, IWSMessageService wsMessageService, IUserService userService)
    {
        _friendShipRepository = friendShipRepository;
        _wsMessageService = wsMessageService;
        _userService = userService;
    }

    public async Task<List<User>> GetFriendsOfUser(string userId)
    {
        return await _friendShipRepository.GetFriendsOfUser(userId);
    }

    public async Task<List<User>> GetIncomingFriendRequestsOfUser(string userId)
    {
        return await _friendShipRepository.GetIncomingFriendRequestsOfUser(userId);
    }

    public async Task<List<User>> GetOutgoingFriendRequestsOfUser(string userId)
    {
        return await _friendShipRepository.GetOutgoingFriendRequestsOfUser(userId);
    }

    public async Task<HttpStatusCode> StoreAndForwardFriendRequest(FriendRequest friendRequest)
    {
        var senderId = friendRequest.initiatorId;
        var receiverId = friendRequest.acceptorId;

        if (receiverId == senderId)
            return HttpStatusCode.BadRequest;
        
        if (await _userService.CheckUserExists(receiverId) == false)
            return HttpStatusCode.NotFound;

        if (await CheckFriendshipExists(senderId, receiverId)) 
            return HttpStatusCode.Conflict;
        
        await StoreFriendRequest(senderId, receiverId);
        var json = JsonSerializer.Serialize(friendRequest);
        await _wsMessageService.SendToUser(receiverId, json);

        return HttpStatusCode.Created;
    }
    
    public async Task<HttpStatusCode> AcceptFriendRequest(FriendRequest friendRequest)
    {
        var initiatorId = friendRequest.initiatorId;
        var acceptorId = friendRequest.acceptorId;

        if (await CheckFriendshipExists(initiatorId, acceptorId) == false)
            return HttpStatusCode.NotFound;

        if (await CheckFriendshipPending(initiatorId, acceptorId) == false)
            return HttpStatusCode.Conflict;
        
        // lookup friend request in database
        var friendship = await _friendShipRepository.GetFriendShip(initiatorId, acceptorId);
        
        // complete this friend request
        await _friendShipRepository.SetFriendShipStatus(friendship, false);

        // let the user who sent out the request know that it has been accepted
        var json = JsonSerializer.Serialize(
            new { friendId = acceptorId, type = "acceptedFriendRequest" } //type = WSMessageType.notification
        );
        
        await _wsMessageService.SendToUser(initiatorId, json);

        return HttpStatusCode.OK;
    }
    
    public async Task<HttpStatusCode> DeclineFriendRequest(FriendRequest friendRequest)
    {
        var initiatorId = friendRequest.initiatorId;
        var acceptorId = friendRequest.acceptorId;
        var statusCode = HttpStatusCode.OK;
        
        // lookup friend request in database
        var friendship = await _friendShipRepository.GetFriendShip(initiatorId, acceptorId);

        if (await CheckFriendshipExists(initiatorId, acceptorId) == false) 
            statusCode = HttpStatusCode.NotFound;
        
        // delete this friend request only if it hasn't already been accepted
        if (await CheckFriendshipPending(initiatorId, acceptorId) == false) 
            statusCode = HttpStatusCode.Conflict;
        
        await _friendShipRepository.RemoveFriendShip(friendship);
        return statusCode;
    }
    
    public async Task<HttpStatusCode> RemoveFriend(string userId1, string userId2)
    {

        if (await CheckFriendshipExists(userId1, userId2) == false)
            return HttpStatusCode.NotFound;
        
        await _friendShipRepository.RemoveFriendShip(userId1, userId2);
        return HttpStatusCode.OK;
    }
    
    public async Task<UserFriendShip> StoreFriendRequest(string senderId, string receiverId)
    {
        var friendship = new UserFriendShip
        {
            UserId = senderId,
            RelatedUserId = receiverId,
            isPending = true
        };

        await _friendShipRepository.AddFriendShip(friendship);

        return friendship;
    }

    public async Task<bool> CheckFriendshipExists(string userId1, string userId2)
    {
        var friendship = await _friendShipRepository.GetFriendShip(userId1, userId2);
        return friendship != null;
    }
    
    public async Task<bool> CheckFriendshipPending(string userId1, string userId2)
    {
        var friendship = await _friendShipRepository.GetFriendShip(userId1, userId2);
        return friendship != null && friendship.isPending;
    }
    
}