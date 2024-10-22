using System.Net.WebSockets;

namespace react_chat_app_backend.Controllers.WSControllers;

public class WSManager : IWSManager
{
    private List<WSClient> _connections = new();

    public void Add(string userId, WebSocket webSocket)
    {
        _connections.Add(new WSClient 
        {
            webSocket = webSocket,
            userId = userId,
        });
        
        LazyClearAbortedConnections();
    }

    public WSClient Get(string userId)
    {
        return _connections.FirstOrDefault(c => c.userId == userId);
    }
    
    public WSClient Get(WebSocket webSocket)
    {
        return _connections.FirstOrDefault(c => c.webSocket == webSocket);
    }

    public void Remove(string userId)
    {
        var i = _connections.FindIndex(c => c.userId == userId);
        _connections.RemoveAt(i);
    }
    
    public void Remove(WebSocket webSocket)
    {
        var i = _connections.FindIndex(c => c.webSocket == webSocket);
        _connections.RemoveAt(i);
    }

    public List<WSClient> All()
    {
        return _connections;
    }

    public void Clear()
    {
        _connections.Clear();
    }

    // Voor een of andere reden aborten sommige connecties in azure, lokaal gebeurd dit niet.
    // Deze connecties blijven voor altijd in de memory, vandaar deze functie
    private void LazyClearAbortedConnections()
    {
        var abortedConnections = All()
                .Where(client => client.webSocket.State == WebSocketState.Aborted)
                .Select(client => client.webSocket);

        foreach (var connection in abortedConnections) {
            Remove(connection);
        }
    }
}