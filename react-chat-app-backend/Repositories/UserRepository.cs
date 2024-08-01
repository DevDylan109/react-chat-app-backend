using System.Net;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories;

public class UserRepository : IUserRepository
{
    private AppDbContext _appDbContext;
    
    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public async Task CreateUser(UserData userData)
    {
        await _appDbContext.Users.AddAsync(userData);
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task<UserData?> GetUser(string userId)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(ur => ur.userId == userId);
    }

    public async Task RemoveUser(UserData user)
    {
        _appDbContext.Remove(user);
        await _appDbContext.SaveChangesAsync();
    }
}