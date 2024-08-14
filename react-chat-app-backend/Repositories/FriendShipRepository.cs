using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;

namespace react_chat_app_backend.Repositories;

public class FriendShipRepository : IFriendShipRepository
{
    private AppDbContext _appDbContext;
    
    public FriendShipRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public async Task<List<User>> GetFriendsOfUser (string userId)
    {
        var list1 = await _appDbContext.UserFriendShips
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RelatedUser)
            .ToListAsync();

        var list2 = await _appDbContext.UserFriendShips
            .Where(ur => ur.RelatedUserId == userId)
            .Select(ur => ur.User)
            .ToListAsync();

        var friends = new List<User>();
        friends.AddRange(list1);
        friends.AddRange(list2);
        
        return friends;
    }

    public async Task<List<User>> GetPotentialFriendsOfUser(string userId)
    {
        return await _appDbContext.UserFriendShips
            .Where(ur => ur.UserId == userId)
            .Where(ur => ur.isPending == true)
            .Select(ur => ur.RelatedUser)
            .ToListAsync();
    }

    public async Task<UserFriendShip?> GetFriendShip(string userId1, string userId2)
    {
        return await _appDbContext.UserFriendShips.FirstOrDefaultAsync(ur =>
            ur.UserId == userId1 && ur.RelatedUserId == userId2 ||
            ur.UserId == userId2 && ur.RelatedUserId == userId1
        );
    }

    public async Task SetFriendShipStatus(UserFriendShip friendShip, bool pendingStatus)
    {
        friendShip.isPending = pendingStatus; 
        _appDbContext.Entry(friendShip).Property(fr => fr.isPending).IsModified = true;
        await _appDbContext.SaveChangesAsync();
    }

    public async Task AddFriendShip(UserFriendShip friendShip)
    {
        await _appDbContext.UserFriendShips.AddAsync(friendShip);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task RemoveFriendShip(UserFriendShip friendShip)
    {
        _appDbContext.UserFriendShips.Remove(friendShip);
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task RemoveFriendShip(string userId1, string userId2)
    {
        await _appDbContext.UserFriendShips
            .Where(ur => ur.UserId == userId1 || ur.UserId == userId2)
            .Where(ur => ur.RelatedUserId == userId1 || ur.RelatedUserId == userId2)
            .ExecuteDeleteAsync();
    }
}