using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace react_chat_app_backend.Models;

public class MessageData
{
    [Key] 
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    
    [DisallowNull]
    public string SenderId { get; set; }
    [DisallowNull]
    public string ReceiverId { get; set; }
    [DisallowNull]
    public string Text { get; set; }

    public MessageData()
    {
        Id = Guid.NewGuid();
        Date = DateTime.UtcNow;
    }
}