using System.Text.Json.Serialization;

namespace react_chat_app_backend.Models;

public class FriendRequest
{
    public string senderId { get; set; }
    public string receiverId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageType type { get; set; } = MessageType.friendRequest;
}