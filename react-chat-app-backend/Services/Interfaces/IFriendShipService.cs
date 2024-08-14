using System.Net;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IFriendShipService
{
     Task<List<User>> GetFriendsOfUser(string userId);
     Task<List<User>> GetPotentialFriendsOfUser(string userId);
     Task<HttpStatusCode> StoreAndForwardFriendRequest(FriendRequest friendRequest);
     Task<HttpStatusCode> AcceptFriendRequest(FriendRequest friendRequest);
     Task<HttpStatusCode> DeclineFriendRequest(FriendRequest friendRequest);
     Task<HttpStatusCode> RemoveFriend(string userId1, string userId2);
     Task<UserFriendShip> StoreFriendRequest(string senderId, string receiverId);
     Task<bool> CheckFriendshipExists(string userId1, string userId2);
     Task<bool> CheckFriendshipPending(string userId1, string userId2);
}