using System.Net;
using System.Text.Json;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;
using react_chat_app_backend.Results;
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

    public async Task<List<User>> GetFriendsOfUser(string userId) => 
        await _friendShipRepository.GetFriendsOfUser(userId);
    

    public async Task<List<User>> GetIncomingFriendRequestsOfUser(string userId) => 
        await _friendShipRepository.GetIncomingFriendRequestsOfUser(userId);
    

    public async Task<List<User>> GetOutgoingFriendRequestsOfUser(string userId) =>
         await _friendShipRepository.GetOutgoingFriendRequestsOfUser(userId);
    

    public async Task<FriendShipResult> StoreAndForwardFriendRequest(string initiatorId, string acceptorId)
    {
        if (acceptorId == initiatorId)
            return FriendShipResult.UserAddSelf();

        if (await _userService.CheckUserExists(acceptorId) == false)
            return FriendShipResult.UserNotFound();

        if (await CheckFriendshipExists(initiatorId, acceptorId)) {
            
            if (await CheckFriendshipPending(initiatorId, acceptorId)) {
                return FriendShipResult.FriendShipIsPending();
            }
            
            return FriendShipResult.FriendShipAlreadyAccepted();
        }
        
        await StoreFriendRequest(initiatorId, acceptorId);
        await _wsMessageService.SendMessage(acceptorId, new { initiatorId, acceptorId, type = "friendRequestReceived" });
        
        return FriendShipResult.FriendShipCreated(acceptorId);
    }
    
    public async Task<FriendShipResult> AcceptFriendRequest(string initiatorId, string acceptorId)
    {
        if (await CheckFriendshipExists(initiatorId, acceptorId) == false)
            return FriendShipResult.FriendShipDoesNotExist();

        if (await CheckFriendshipPending(initiatorId, acceptorId) == false)
            return FriendShipResult.FriendShipAlreadyAccepted();
        
        // lookup friend request in database
        var friendship = await _friendShipRepository.GetFriendShip(initiatorId, acceptorId);
        
        // complete this friend request
        await _friendShipRepository.SetFriendShipStatus(friendship, false);

        // let the user who sent out the request know that it has been accepted
        await _wsMessageService.SendMessage(initiatorId, new { acceptorId, type = "friendRequestResponse" });
        
        return FriendShipResult.FriendRequestAccepted();
    }
    
    public async Task<FriendShipResult> DeclineFriendRequest(string initiatorId, string acceptorId)
    {
        // lookup friend request in database
        var friendship = await _friendShipRepository.GetFriendShip(initiatorId, acceptorId);

        if (await CheckFriendshipExists(initiatorId, acceptorId) == false)
            return FriendShipResult.FriendShipDoesNotExist();
        
        // delete this friend request only if it hasn't already been accepted
        if (await CheckFriendshipPending(initiatorId, acceptorId) == false) 
            return FriendShipResult.FriendShipAlreadyAccepted();
        
        await _friendShipRepository.RemoveFriendShip(friendship);
        await _wsMessageService.SendMessage(initiatorId, new { declinerId = acceptorId, type = "friendRequestResponse" });
        
        return FriendShipResult.FriendRequestDeclined();
    }
    
    public async Task<FriendShipResult> RemoveFriend(string userId1, string userId2)
    {
        if (await CheckFriendshipExists(userId1, userId2) == false)
            return FriendShipResult.FriendShipDoesNotExist();
        
        await _friendShipRepository.RemoveFriendShip(userId1, userId2);
        await _wsMessageService.SendMessage(userId2, new { userId = userId1, type = "removedFriend" });
        
        return FriendShipResult.FriendShipDeleted();
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