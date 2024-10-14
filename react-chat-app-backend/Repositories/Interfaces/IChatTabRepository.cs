using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories.Interfaces;

public interface IChatTabRepository
{
    Task<ChatTab> GetChatTab(string userId);
    Task CreateChatTab(string userId, int unreadMessageCount, bool isHighlighted);
    Task RemoveChatTab(string userId);
}