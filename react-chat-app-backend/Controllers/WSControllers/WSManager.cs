using System.Net.WebSockets;

namespace react_chat_app_backend.Controllers.WSControllers;

public class WSManager : IWSManager
{
    private Dictionary<string, WebSocket> _connections = new();

    public void Add(string userId, WebSocket webSocket)
    {
        _connections.Add(userId, webSocket);
    }

    public WebSocket? Get(string userId)
    {
        _connections.TryGetValue(userId, out var webSocket);
        return webSocket;
    }

    public void Remove(string userId)
    {
        _connections.Remove(userId);
    }

    public List<WebSocket> All()
    {
        return _connections.Values.ToList();
    }

    public void Clear()
    {
        _connections.Clear();
    }
}