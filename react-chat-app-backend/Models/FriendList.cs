using System.Text.Json.Serialization;

namespace react_chat_app_backend.Models;

public class FriendList
{
    public List<UserData> friends { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageType type { get; set; } = MessageType.friendList;
}