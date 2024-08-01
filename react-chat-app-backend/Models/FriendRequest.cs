using System.Text.Json.Serialization;

namespace react_chat_app_backend.Models;

public class FriendRequest
{
    public string initiatorId { get; set; }
    public string acceptorId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageType type { get; set; } = MessageType.friendRequest;
}