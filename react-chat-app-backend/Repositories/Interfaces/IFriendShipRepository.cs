using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories.Interfaces;

public interface IFriendShipRepository
{
    Task<List<User>> GetFriendsOfUser(string userId);
    Task<List<User>> GetIncomingFriendRequestsOfUser(string userId);
    Task<List<User>> GetOutgoingFriendRequestsOfUser(string userId);
    Task<UserFriendShip?> GetFriendShip(string userId1, string userId2);
    Task SetFriendShipStatus(UserFriendShip friendShip, bool pendingStatus);
    Task AddFriendShip(UserFriendShip friendShip);
    Task RemoveFriendShip(UserFriendShip friendShip);
    Task RemoveFriendShip(string userId1, string userId2);

}