using react_chat_app_backend.Models;

namespace react_chat_app_backend.Repositories;

public interface IMessageRepository
{
    Task AddMessage(MessageData message);
    Task<List<MessageData>> GetMessages(string userId1, string userId2);
}