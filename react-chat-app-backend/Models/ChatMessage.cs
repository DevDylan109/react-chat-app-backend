using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace react_chat_app_backend.Models;

public class ChatMessage
{
    [Key] 
    public Guid id { get; set; }

    public DateTime date { get; set; }
    
    public string senderId { get; set; }
    public string receiverId { get; set; }
    public string name { get; set; }
    public string text { get; set; }
    public string? photoURL { get; set; }

    [NotMapped] public string token { get; set; }

    public ChatMessage()
    {
        id = Guid.NewGuid();
        date = DateTime.UtcNow;
    }
    
}