namespace react_chat_app_backend.Models;

public class ChatMessageHistoryRequest
{
    public string userId1 { get; set; }
    public string userId2 { get; set; }
    public int skip { get; set; }
    public int take { get; set; }
}