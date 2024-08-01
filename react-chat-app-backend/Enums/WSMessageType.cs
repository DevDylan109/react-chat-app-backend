using System.Runtime.Serialization;

namespace react_chat_app_backend.Models;

public enum WSMessageType
{
    chatMessage,
    chatHistory,
    friendList,
    friendRequest,
}