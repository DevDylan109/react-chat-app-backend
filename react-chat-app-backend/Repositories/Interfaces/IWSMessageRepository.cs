using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories.Interfaces;

public interface IWSMessageRepository
{
    Task AddMessage(ChatMessage chatMessage);
    Task<List<ChatMessage>> GetMessages(string userId1, string userId2);
}