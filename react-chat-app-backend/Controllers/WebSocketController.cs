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
        
        if (_connections.ContainsKey(userId) == false) 
        {
            _connections.Add(userId, webSocket);
        } 
    }

    private T GetModelData<T>(byte[] buffer)
    {
        var json = Encoding.UTF8.GetString(buffer).Trim('\0');
        return JsonSerializer.Deserialize<T>(json);
    }

    private MessageType GetMessageType(byte[] buffer)
    {
        var typeStr = GetModelData<ModelType>(buffer).type;
        return Enum.Parse<MessageType>(typeStr);
    }

    private async Task<WebSocketReceiveResult> GetReceiveResult(byte[] buffer, WebSocket webSocket)
    {
        return await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    
    private async Task CloseConnection(WebSocket webSocket, WebSocketReceiveResult receiveResult)
    {
        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
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
                WebSocketMessageFlags.EndOfMessage,
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
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
            );
    }

    private async Task FetchMessageHistory(WebSocket webSocket, byte[] buffer)
    {
        var userIds = GetModelData<UsersData>(buffer);
        var userId1 = userIds.userId1;
        var userId2 = userIds.userId2;

        var messages = await _appDbContext.Messages.Where(m =>
            m.senderId == userId1 && m.receiverId == userId2 ||
            m.senderId == userId2 && m.receiverId == userId1)
            .ToListAsync();
        
        var messageHistory = new MessageHistory
        {
            messages = messages,
            type = MessageType.chatHistory
        };
        
        var json = JsonSerializer.Serialize(messageHistory);  
        buffer = Encoding.UTF8.GetBytes(json);

        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
            );
    }

    private async Task StoreMessage(byte[] buffer)
    {
        var message = GetModelData<MessageData>(buffer);
        _appDbContext.Messages.Add(message);
        await _appDbContext.SaveChangesAsync();
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
            
            var messageType = GetMessageType(buffer);
            switch (messageType)
            {
                case MessageType.chatMessage: 
                    await StoreMessage(buffer);
                    await ForwardMessage(buffer);
                    break;
                
                case MessageType.Register:
                    RegisterConnection(webSocket, buffer);
                    break;
                
                case MessageType.chatHistory:
                    await FetchMessageHistory(webSocket, buffer);
                    break;
            }
        }
    }
}