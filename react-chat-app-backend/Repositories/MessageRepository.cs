using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories;

public class MessageRepository : IMessageRepository
{
    private AppDbContext _appDbContext;

    public MessageRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task AddMessage(MessageData message)
    {
        _appDbContext.Messages.Add(message);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<List<MessageData>> GetMessages(string userId1, string userId2)
    {
         return await _appDbContext.Messages.Where(m =>
                m.senderId == userId1 && m.receiverId == userId2 ||
                m.senderId == userId2 && m.receiverId == userId1)
            .ToListAsync();
    }
    
}