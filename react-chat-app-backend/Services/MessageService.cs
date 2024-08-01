using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using react_chat_app_backend.Controllers;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories;

namespace react_chat_app_backend.Services;

public class MessageService
{
    private IMessageRepository _messageRepository;
    private FriendShipService _friendShipService;

    public MessageService(IMessageRepository messageRepository, FriendShipService friendShipService)
    {
        _messageRepository = messageRepository;
        _friendShipService = friendShipService;
    }
    
    public async Task FetchMessageHistory(WebSocket webSocket, byte[] buffer)
    {
        var userIds = GetModelData<UsersData>(buffer);
        var userId1 = userIds.userId1;
        var userId2 = userIds.userId2;

        var messages = await _messageRepository.GetMessages(userId1, userId2);
        var messageHistory = new { messages, type = MessageType.chatHistory };
        buffer = ToJsonByteArray(messageHistory);

        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }
    
    public async Task StoreMessage(MessageData message)
    {
        await _messageRepository.AddMessage(message);
    }
    
    public async Task ForwardMessage(MessageData message)
    {
        var receiverId = message.receiverId;
        var isReceiverConnected = WebSocketController.Connections.TryGetValue(receiverId, out var receiverSocket);
        
        message.date = DateTime.UtcNow;
        
        var buffer = ToJsonByteArray(message);
        
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
    
    public async Task StoreAndForwardMessage(byte[] buffer)
    {
        var message = GetModelData<MessageData>(buffer);
        var senderId = message.senderId;
        var receiverId = message.receiverId;
        
        if (await _friendShipService.CheckFriendshipExists(senderId, receiverId))
        {
            await StoreMessage(message);
            await ForwardMessage(message);   
        }
    }
    
    public static async Task SendMessageToUser(string userId, string json)
    {
        WebSocketController.Connections.TryGetValue(userId, out var connection);

        if (connection != null)
        {
            var buffer = Encoding.UTF8.GetBytes(json);
        
            await connection.SendAsync(
                new ArraySegment<byte>(buffer, 0, buffer.Length),
                WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None
            );
        }
    }
    
    public byte[] ToJsonByteArray(object obj)
    {
        var json = JsonSerializer.Serialize(obj);  
        return Encoding.UTF8.GetBytes(json);
    }
    
    public T GetModelData<T>(byte[] buffer)
    {
        var json = Encoding.UTF8.GetString(buffer).Trim('\0');
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public int GetLengthWithoutPadding(byte[] buffer)
    {
        var i = buffer.Length - 1;
        
        while (i >= 0 && buffer[i] == 0)
            i--;

        return i + 1;
    }
    
    public MessageType GetMessageType(byte[] buffer)
    {
        var typeStr = GetModelData<ModelType>(buffer).type;
        return Enum.Parse<MessageType>(typeStr);
    }
}