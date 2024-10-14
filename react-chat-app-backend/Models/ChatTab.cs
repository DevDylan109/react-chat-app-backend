using System.ComponentModel.DataAnnotations;

namespace react_chat_app_backend.Models;

public class ChatTab
{
    [Key]
    public Guid id { get; set; }
    public string userId { get; set; }
    public bool isHighlighted { get; set; }
    public int unreadMessageCount { get; set; }
    
    public ChatTab()
    {
        id = Guid.NewGuid();
    }
}