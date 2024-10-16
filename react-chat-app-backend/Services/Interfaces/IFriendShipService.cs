using System.Net;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IFriendShipService
{
     Task<List<User>> GetFriendsOfUser(string userId);
     Task<List<User>> GetIncomingFriendRequestsOfUser(string userId);
     Task<List<User>> GetOutgoingFriendRequestsOfUser(string userId);
     Task<HttpStatusCode> StoreAndForwardFriendRequest(string initiatorId, string acceptorId);
     Task<HttpStatusCode> AcceptFriendRequest(string initiatorId, string acceptorId);
     Task<HttpStatusCode> DeclineFriendRequest(string initiatorId, string acceptorId);
     Task<HttpStatusCode> RemoveFriend(string userId1, string userId2);
     Task<UserFriendShip> StoreFriendRequest(string senderId, string receiverId);
     Task<bool> CheckFriendshipExists(string userId1, string userId2);
     Task<bool> CheckFriendshipPending(string userId1, string userId2);
}