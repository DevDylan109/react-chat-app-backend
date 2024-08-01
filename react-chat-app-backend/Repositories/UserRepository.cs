using System.Net;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;

namespace react_chat_app_backend.Repositories;

public class UserRepository : IUserRepository
{
    private AppDbContext _appDbContext;
    
    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public async Task CreateUser(User user)
    {
        await _appDbContext.Users.AddAsync(user);
        await _appDbContext.SaveChangesAsync();
    }
    
    public async Task<User?> GetUser(string userId)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(ur => ur.userId == userId);
    }

    public async Task RemoveUser(User user)
    {
        _appDbContext.Remove(user);
        await _appDbContext.SaveChangesAsync();
    }
}