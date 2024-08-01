using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;
using react_chat_app_backend.Services;

namespace react_chat_app_backend.Controllers;

public class WebSocketController : ControllerBase
{
    public static Dictionary<string, WebSocket> Connections = new ();
    private AppDbContext _appDbContext;
    private MessageService _messageService;
    
    public WebSocketController(AppDbContext dbContext, MessageService messageService)
    {
        _appDbContext = dbContext;
        _messageService = messageService;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            await _appDbContext.SaveChangesAsync();
            
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleMessage(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    
    private void RegisterConnection(WebSocket webSocket, byte[] buffer)
    {
        var registerData = _messageService.GetModelData<RegisterData>(buffer);
        var userId = registerData.userId;
        
        if (Connections.ContainsKey(userId) == false) 
        {
            Connections.Add(userId, webSocket);
        } 
    }
    
    private async Task CloseConnection(WebSocket webSocket, WebSocketReceiveResult receiveResult)
    {
        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private async Task HandleMessage(WebSocket webSocket)
    {
        while (true)
        {
            var buffer = new byte[1024 * 4];
            
            var receiveResult = await GetReceiveResult(buffer, webSocket);
            if (receiveResult.CloseStatus.HasValue)
            {
                await CloseConnection(webSocket, receiveResult);
                break;   
            }
            
            var messageType = _messageService.GetMessageType(buffer);
            switch (messageType)
            {
                case MessageType.register:
                    RegisterConnection(webSocket, buffer);
                    break;
                
                case MessageType.chatMessage:
                    await _messageService.StoreAndForwardMessage(buffer);
                    break;
                
                case MessageType.chatHistory:
                    await _messageService.FetchMessageHistory(webSocket, buffer);
                    break;
            }
        }
    }
    
    private async Task<WebSocketReceiveResult> GetReceiveResult(byte[] buffer, WebSocket webSocket)
    {
        return await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }
}