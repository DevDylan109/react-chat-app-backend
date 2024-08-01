using System.Runtime.CompilerServices;

namespace react_chat_app_backend.Models;

public class UserFriendShip
{
    public string UserId { get; set; }
    public User User { get; set; }

    public string RelatedUserId { get; set; }
    public User RelatedUser { get; set; }

    public bool isPending { get; set; }
}