using System.ComponentModel.DataAnnotations;

namespace react_chat_app_backend.Models;

public class User
{
    
    [Key]
    public string userId { get; set; }
    public string password { get; set; }
    public string? photoURL { get; set; }
    public string name { get; set; }
    public string? lastMessage { get; set; }
    public DateTime? joinDate { get; set; }

    public List<UserFriendShip>? UserFriendShips { get; set; }

    public User()
    {
        joinDate = DateTime.UtcNow;
    }
}