using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using react_chat_app_backend.Services;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Controllers.WSControllers;

public class WSController : ControllerBase
{
    private IWSManager _wsManager;
    private IWSMessageService _wsMessageService;
    
    public WSController(IWSMessageService wsMessageService, IWSManager wsManager)
    {
        _wsManager = wsManager;
        _wsMessageService = wsMessageService;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            string userId = HttpContext.Request.Query["userID"];
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"user id: {userId}");
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _wsManager.Add(userId, webSocket);
                await Listener(webSocket);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    private async Task Listener(WebSocket webSocket)
    {
        while (true)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket
                .ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
            
            if (receiveResult.CloseStatus.HasValue)
            {
                _wsManager.Remove(webSocket);
                
                await webSocket.CloseAsync(
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                break;   
            }
            await _wsMessageService.Handle(webSocket, buffer);
        }
    }
    
}