using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using react_chat_app_backend.Context;
using react_chat_app_backend.Migrations;
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

    private async Task StoreMessage(MessageData message)
    {
        _appDbContext.Messages.Add(message);
        await _appDbContext.SaveChangesAsync();
    }
    
    private async Task ForwardMessage(MessageData message)
    {
        var receiverId = message.receiverId;
        var isReceiverConnected =_connections.TryGetValue(receiverId, out var receiverSocket);
        
        message.date = DateTime.UtcNow;
        
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        
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

    private async Task StoreAndForwardMessage(byte[] buffer)
    {
        var message = GetModelData<MessageData>(buffer);
        var senderId = message.senderId;
        var receiverId = message.receiverId;
        
        if (await CheckFriendshipExists(senderId, receiverId))
        {
            await StoreMessage(message);
            await ForwardMessage(message);   
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

    private async Task<UserFriendShip?> GetFriendship(string userId1, string userId2)
    {
        return await _appDbContext.UserFriendShips.FirstOrDefaultAsync(ur =>
            ur.UserId == userId1 && ur.RelatedUserId == userId2 ||
            ur.UserId == userId2 && ur.RelatedUserId == userId1
        );
    }

    private async Task<bool> CheckFriendshipExists(string userId1, string userId2)
    {
        var friendship = await GetFriendship(userId1, userId2);
        return friendship != null;
    }
    
    private async Task<bool> CheckFriendshipPending(string userId1, string userId2)
    {
        var friendship = await GetFriendship(userId1, userId2);
        return friendship != null && friendship.isPending;
    }
    
    private async Task StoreFriendRequest(FriendRequest friendRequest)
    {
        var senderId = friendRequest.senderId;
        var receiverId = friendRequest.receiverId;

        var friendship = new UserFriendShip
        {
            UserId = senderId,
            RelatedUserId = receiverId,
            isPending = true
        };

        await _appDbContext.UserFriendShips.AddAsync(friendship);
        await _appDbContext.SaveChangesAsync();
    }

    private async Task ForwardFriendRequest(string receiverId, byte[] buffer)
    {
        var isReceiverConnected = _connections.TryGetValue(receiverId, out var receiverConnection);
        if (isReceiverConnected)
        {
            var bufferLength = GetLengthWithoutPadding(buffer);
            
            await receiverConnection.SendAsync(
                new ArraySegment<byte>(buffer, 0, bufferLength),
                WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None
            );
        }
    }

    private async Task StoreAndForwardFriendRequest(byte[] buffer)
    {
        var friendRequest = GetModelData<FriendRequest>(buffer);
        var senderId = friendRequest.senderId;
        var receiverId = friendRequest.receiverId;
        
        if (await CheckFriendshipExists(senderId, receiverId)) {
            return;
        }

        await StoreFriendRequest(friendRequest);
        await ForwardFriendRequest(receiverId, buffer);
    }

    private async Task AcceptFriendRequest(WebSocket webSocket, byte[] buffer)
    {
        var friendRequest = GetModelData<FriendRequest>(buffer);
        var userId1 = friendRequest.senderId;
        var userId2 = friendRequest.receiverId;
        
        // lookup friend request in database
        var friendship = await GetFriendship(userId1, userId2);
        
        // complete this friend request
        friendship.isPending = false; 
        _appDbContext.Entry(friendship).Property(fr => fr.isPending).IsModified = true;
        await _appDbContext.SaveChangesAsync();

        // send a notification to the user who has sent the friend request if he's connected
        var jsonObject = new
        {
            text = $"{userId2} has accepted your friend request!",
            type = "notification"
            //type = MessageType.notification
        };

        var jsonString = JsonSerializer.Serialize(jsonObject);
        buffer = Encoding.UTF8.GetBytes(jsonString);
        
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }

    private async Task DeclineFriendRequest(byte[] buffer)
    {
        var friendRequest = GetModelData<FriendRequest>(buffer);
        var userId1 = friendRequest.senderId;
        var userId2 = friendRequest.receiverId;
        
        // lookup friend request in database
        var friendship = await GetFriendship(userId1, userId2);

        if (await CheckFriendshipExists(userId1, userId2))
        {
            if (await CheckFriendshipPending(userId1, userId2))
            {
                // delete this friend request only if it hasn't already been accepted
                _appDbContext.Remove(friendship);
                await _appDbContext.SaveChangesAsync();   
            }
        }
    }

    private async Task RemoveFriend(byte[] buffer)
    {
        var usersData = GetModelData<UsersData>(buffer);
        var userId1 = usersData.userId1;
        var userId2 = usersData.userId2;

       await _appDbContext.UserFriendShips
            .Where(ur => ur.UserId == userId1 || ur.UserId == userId2)
            .Where(ur => ur.RelatedUserId == userId1 || ur.RelatedUserId == userId2)
            .ExecuteDeleteAsync();
    }

    private async Task FetchAllFriends(WebSocket webSocket, byte[] buffer)
    {
        var userData = GetModelData<UserData>(buffer);
        var userId = userData.userId;
        
        var list1 = await _appDbContext.UserFriendShips
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RelatedUser)
            .ToListAsync();

        var list2 = await _appDbContext.UserFriendShips
            .Where(ur => ur.RelatedUserId == userId)
            .Select(ur => ur.User)
            .ToListAsync();

        var friends = new List<UserData>();
        friends.AddRange(list1);
        friends.AddRange(list2);
        
        var friendList = new FriendList
        {
            friends = friends
        };
        
        var json = JsonSerializer.Serialize(friendList);
        buffer = Encoding.UTF8.GetBytes(json);

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
                    await StoreAndForwardMessage(buffer);
                    break;
                
                case MessageType.register:
                    RegisterConnection(webSocket, buffer);
                    break;
                
                case MessageType.chatHistory:
                    await FetchMessageHistory(webSocket, buffer);
                    break;
                
                case MessageType.friendRequest:
                    await StoreAndForwardFriendRequest(buffer);
                    break;
                
                case MessageType.acceptFriendRequest:
                    await AcceptFriendRequest(webSocket, buffer);
                    break;
                
                case MessageType.declineFriendRequest:
                    await DeclineFriendRequest(buffer);
                    break;
                
                case MessageType.removeFriend:
                    await RemoveFriend(buffer);
                    break;
                
                case MessageType.friendList:
                    await FetchAllFriends(webSocket, buffer);
                    break;
            }
        }
    }
}