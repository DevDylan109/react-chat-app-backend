using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace react_chat_app_backend.Models;

public class MessageData
{
    [Key]
    public string senderId { get; set; }
    public string receiverId { get; set; }
    public string text { get; set; }

    public DateTime date { get; set; }
}