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
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
                _wsManager.Add(userId, webSocket);
                await _wsMessageService.BroadcastMessage(userId,new { userId, status = "online", type = "friendStatus" });
                
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
                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket
                    .ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
            
                if (receiveResult.CloseStatus.HasValue) {
                    await webSocket.CloseAsync(
                        receiveResult.CloseStatus.Value,
                        receiveResult.CloseStatusDescription,
                        CancellationToken.None);
                    break;   
                }

                // Get the string without trailing bytes, due to buffer being larger than amount received
                var str = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            
                if (str == "ping") 
                    await Pong(webSocket);
                else 
                    await _wsMessageService.Handle(webSocket, buffer);
            }
        }
        catch (Exception e) {
            // Exception occurs when a client loses internet connection, I want to ignore this and continue.
        }
        
        await HandleClose(webSocket);
    }
    
    // The client pings the server to check if his connection timed out.
    private async Task Pong(WebSocket webSocket)
    {
        var str = "pong";
        var buffer = Encoding.UTF8.GetBytes(str);
        
        await webSocket.SendAsync(buffer,
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }

    private async Task HandleClose(WebSocket webSocket)
    {
        var userId = _wsManager.Get(webSocket).userId;
        
        await _wsMessageService.BroadcastMessage(userId, new { userId, status = "offline", type="friendStatus" });
        _wsManager.Remove(webSocket);
    }

}