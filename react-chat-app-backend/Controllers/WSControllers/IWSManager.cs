using System.Net.WebSockets;

namespace react_chat_app_backend.Controllers.WSControllers;

public interface IWSManager
{
     void Add(string userId, WebSocket webSocket);
     WSClient Get(string userId);
     WSClient Get(WebSocket webSocket);
     void Remove(string userId);
     void Remove(WebSocket webSocket);
     List<WSClient> All();
     void Clear();
}