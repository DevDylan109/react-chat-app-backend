using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories.Interfaces;

public interface IWSMessageRepository
{
    Task StoreMessage(ChatMessage chatMessage);
    Task<List<ChatMessage>> GetAllMessages(string userId1, string userId2);
    Task<List<ChatMessage>> GetMessageSequence(string userId1, string userId2, int skip, int take);
}