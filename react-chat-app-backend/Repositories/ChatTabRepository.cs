using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;

namespace react_chat_app_backend.Repositories;

public class ChatTabRepository : IChatTabRepository
{
    private AppDbContext _appDbContext;

    public ChatTabRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<ChatTab> GetChatTab(string userId)
    {
        return await _appDbContext.ChatTabs.FirstOrDefaultAsync(ct => ct.userId == userId);
    }

    public async Task CreateChatTab(string userId, int unreadMessageCount, bool isHighlighted)
    {
        await _appDbContext.AddAsync(new ChatTab
        {
            userId = userId,
            isHighlighted = isHighlighted,
            unreadMessageCount = unreadMessageCount
        });
        await _appDbContext.SaveChangesAsync();
    }

    public async Task RemoveChatTab(string userId)
    {
        var chatTab = await GetChatTab(userId);
        _appDbContext.Remove(chatTab);
        await _appDbContext.SaveChangesAsync();
    }
}