using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Controllers;

public class WebSocketController : ControllerBase
{
    private static Dictionary<string, WebSocket> _connections = new ();
    
    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleMessage(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    
    private void RegisterConnection(string userId, WebSocket webSocket) 
    {
        if (_connections.ContainsKey(userId)) 
        {
            Console.WriteLine("this connection is already registered");
        } 
        else 
        {
            _connections.Add(userId, webSocket);
            Console.WriteLine($"Registered: user: {userId}, {webSocket.State}");
            Console.WriteLine("amount of current connections: " + _connections.Count);
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

    private async Task ForwardMessage(WebSocket webSocket, byte[] buffer)
    {
        var message = GetModelData<MessageData>(buffer);
        var receiverId = message.receiverId;
        var receiverWebSocket = _connections?[receiverId];

        if (receiverWebSocket == null)
        {
            // no socket found for this userId
           // continue;
        }

        message.text = $"Forwarded message from: {message.senderId} to {message.receiverId}. {message.text}";
            
        Encoding utf8Encoding = Encoding.UTF8;
        buffer = utf8Encoding.GetBytes(message.text);
            
        // await receiverWebSocket.SendAsync(
        //     new ArraySegment<byte>(buffer, 0, buffer.Length),
        //     receiveResult.MessageType,
        //     receiveResult.EndOfMessage,
        //     CancellationToken.None);
    }

    private async Task HandleMessage(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await GetReceiveResult(buffer, webSocket);
        var registerMessage = GetModelData<RegisterData>(buffer);
        
        if (registerMessage.type == "register")
        {
            RegisterConnection(registerMessage.userId, webSocket);
        }
        else
        {
            Console.WriteLine(" this is not a register message ");
        }
        
        while (!receiveResult.CloseStatus.HasValue)
        {
            buffer = new byte[1024 * 4];
            receiveResult = await GetReceiveResult(buffer, webSocket);

            var message = GetModelData<ModelType>(buffer);
            
            if (message.type == "message")
            {
                await ForwardMessage(webSocket, buffer);
            }
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
    
}