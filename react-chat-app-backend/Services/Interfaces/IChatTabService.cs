using react_chat_app_backend.Controllers.HttpControllers;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IChatTabService
{
    Task<ChatTab> GetChatTab(string userId);
    Task SaveChatTab(string userId, int unreadMessageCount, bool isHighlighted);
    Task DeleteChatTab(string userId);
}