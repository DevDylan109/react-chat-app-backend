using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Context;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Controllers;

public class WebSocketController : ControllerBase
{
    private static Dictionary<string, WebSocket> _connections = new ();
    private AppDbContext _appDbContext;
    
    public WebSocketController(AppDbContext dbContext)
    {
        _appDbContext = dbContext;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            _appDbContext.Messages.Add(new MessageData { receiverId = "test2", senderId = "TEst3", text = "Test4" });
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
        var registerData = GetModelData<RegisterData>(buffer);
        var userId = registerData.userId;
        
        if (_connections.ContainsKey(userId)) 
        {
            Console.WriteLine("this connection is already registered");
        } 
        else 
        {
            _connections.Add(userId, webSocket);
            Console.WriteLine($"Registered: user: {userId}, {webSocket.State}");
        }
    }

    private T GetModelData<T>(byte[] buffer)
    {
        var json = Encoding.UTF8.GetString(buffer).Trim('\0');
        return JsonSerializer.Deserialize<T>(json);
    }

    private async Task<WebSocketReceiveResult> GetReceiveResult(byte[] buffer, WebSocket webSocket)
    {
        return await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    private int GetLengthWithoutPadding(byte[] buffer)
    {
        var i = buffer.Length - 1;
        
        while (i >= 0 && buffer[i] == 0)
            i--;

        return i + 1;
    }

    private async Task ForwardMessage(byte[] buffer)
    {
        var message = GetModelData<MessageData>(buffer);
        var receiverId = message.receiverId;
        var isReceiverConnected =_connections.TryGetValue(receiverId, out var receiverSocket);
        
        if (isReceiverConnected)
        {
            var bufferLength = GetLengthWithoutPadding(buffer);
            
            await receiverSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, bufferLength),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }

    private async Task SendResponse(WebSocket webSocket, string json)
    {
        var buffer = Encoding.UTF8.GetBytes(json);
        
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
            );
    }

    private async Task FetchMessageHistory(WebSocket webSocket)
    {
        var messages =_appDbContext.Messages.ToString();
        var buffer = Encoding.UTF8.GetBytes(messages);

        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
            );
    }

    private async Task HandleMessage(WebSocket webSocket)
    {
        while (true)
        {
            var buffer = new byte[1024 * 4];
            
            var receiveResult = await GetReceiveResult(buffer, webSocket);
            if (receiveResult.CloseStatus.HasValue)
            {
                await webSocket.CloseAsync(
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                break;   
            }
            
            var message = GetModelData<ModelType>(buffer);
            switch (message.type)
            {
                case "textMessage": 
                    await ForwardMessage(buffer);
                    await SendResponse(webSocket, "{ \"isDelivered\":\"true\" }");
                    break;
                case "register":
                    RegisterConnection(webSocket, buffer);
                    await SendResponse(webSocket, "{ \"isRegistered\":\"true\" }"); 
                    break;
                case "messageHistory":
                    await FetchMessageHistory(webSocket);
                    break;
            }
        }
    }
    
}