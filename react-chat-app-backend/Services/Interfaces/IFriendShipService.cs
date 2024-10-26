using System.Net;
using react_chat_app_backend.Models;
using react_chat_app_backend.Results;

namespace react_chat_app_backend.Services.Interfaces;

public interface IFriendShipService
{
     Task<List<User>> GetFriendsOfUser(string userId);
     Task<List<User>> GetIncomingFriendRequestsOfUser(string userId);
     Task<List<User>> GetOutgoingFriendRequestsOfUser(string userId);
     Task<FriendShipResult> StoreAndForwardFriendRequest(string initiatorId, string acceptorId);
     Task<FriendShipResult> AcceptFriendRequest(string initiatorId, string acceptorId);
     Task<FriendShipResult> DeclineFriendRequest(string initiatorId, string acceptorId);
     Task<FriendShipResult> RemoveFriend(string userId1, string userId2);
     Task<UserFriendShip> StoreFriendRequest(string senderId, string receiverId);
     Task<bool> CheckFriendshipExists(string userId1, string userId2);
     Task<bool> CheckFriendshipPending(string userId1, string userId2);
}