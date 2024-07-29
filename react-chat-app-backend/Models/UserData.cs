using System.ComponentModel.DataAnnotations;

namespace react_chat_app_backend.Models;

public class UserData
{
    
    [Key]
    public string userId { get; set; }
    public string password { get; set; }
    public string? photoURL { get; set; }
    public string name { get; set; }
    public string? lastMessage { get; set; }

    public List<UserFriendShip>? UserFriendShips { get; set; }
}