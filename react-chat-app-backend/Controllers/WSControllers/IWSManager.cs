using System.Net.WebSockets;

namespace react_chat_app_backend.Controllers.WSControllers;

public interface IWSManager
{
     void Add(string userId, WebSocket webSocket);
     WebSocket? Get(string userId);
     void Remove(string userId);
     List<WebSocket> All();
     void Clear();
}