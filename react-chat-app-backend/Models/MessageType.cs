using System.Runtime.Serialization;

namespace react_chat_app_backend.Models;

public enum MessageType
{
    chatMessage,
    chatHistory,
    friendList,
    friendRequest,
    acceptFriendRequest,
    declineFriendRequest,
    removeFriend,
    register,
    notification
}