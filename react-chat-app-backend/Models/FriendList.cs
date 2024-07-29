namespace react_chat_app_backend.Models;

public class FriendList
{
    public List<string> friendIds { get; set; }

    public MessageType type = MessageType.friendList;
}