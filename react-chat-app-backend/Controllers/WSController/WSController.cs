using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Controllers.WSControllers;

public class WSController : ControllerBase
{
    private IWSManager _wsManager;
    private IWSMessageService _wsMessageService;
    private ITokenService _tokenService;
    
    private Timer _timer;
    private DateTime _lastPingDate = DateTime.Now;
    private int _pingInterval = 30000;
    
    public WSController(IWSMessageService wsMessageService, IWSManager wsManager, ITokenService tokenService)
    {
        _wsManager = wsManager;
        _wsMessageService = wsMessageService;
        _tokenService = tokenService;
    }

    [Route("/ws")]
    public async Task Get()
    {
        string userId = HttpContext.Request.Query["userID"];
        string token = HttpContext.Request.Query["token"];

        if (HttpContext.WebSockets.IsWebSocketRequest 
            && _tokenService.IsTokenValid(userId, token)) {
            
            if (!string.IsNullOrEmpty(userId)) {
                
                // Handshake
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
                // Cache userid with socket, so that server knows which socket belongs to which user
                _wsManager.Add(userId, webSocket);
                
                // Start timer window for checking if a wsocket connection timed out
                _timer = new Timer(CheckTimeout, webSocket, _pingInterval, _pingInterval);
                
                // Notify currently connected users that this user connected/went online
                await _wsMessageService.BroadcastMessage(userId,new { userId, status = "online", type = "friendStatus" });
                
                // Start listening to incoming wsocket messages
                await Listener(webSocket);
            }
        } else {
            
            HttpContext.Response.StatusCode = 400;
        }
    }

    private async Task Listener(WebSocket webSocket)
    {
        try
        {
            while (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
            {
                
                // Cache the incoming websocket message
                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket
                    .ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
            
                // Checks if client closed connection
                if (receiveResult.CloseStatus.HasValue) {
                    await webSocket.CloseAsync(
                        receiveResult.CloseStatus.Value,
                        receiveResult.CloseStatusDescription,
                        CancellationToken.None);
                    break;   
                }
                
                // Get the string without trailing bytes, due to buffer being larger than amount receive,
                // to make strings compare correctly
                var str = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            
                if (str == "ping") 
                    await Pong(webSocket);
                else 
                    await _wsMessageService.HandleIncomingMessage(webSocket, buffer);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        await HandleClose(webSocket);
    }
    
    // The client pings the server to check if his connection timed out.
    private async Task Pong(WebSocket webSocket)
    {
        var str = "pong";
        var buffer = Encoding.UTF8.GetBytes(str);

        _lastPingDate = DateTime.Now;
        
        await webSocket.SendAsync(buffer,
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }

    // Abort the connection when client hasn't sent a ping within the timeframe
    private void CheckTimeout(object webSocket)
    {
        var socket = (WebSocket) webSocket;
        var timePassed = DateTime.Now - _lastPingDate;
        
        if (timePassed.Milliseconds > _pingInterval) {
            socket.Abort();
            _timer.DisposeAsync();
        }
    }

    private async Task HandleClose(WebSocket webSocket)
    {
        // Notify currently connected user that this user disconnected/went offline
        var userId = _wsManager.Get(webSocket).userId;
        await _wsMessageService.BroadcastMessage(userId, new { userId, status = "offline", type="friendStatus" });
        
        await _timer.DisposeAsync();
        
        // Remove unused wsocket from cache
        _wsManager.Remove(webSocket);
    }

}