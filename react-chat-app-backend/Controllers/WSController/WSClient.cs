using System.Net.WebSockets;

namespace react_chat_app_backend.Controllers.WSControllers;

public class WSClient
{
    public WebSocket webSocket { get; set; }
    public string userId { get; set; }
}