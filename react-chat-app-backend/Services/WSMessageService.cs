using System.Net.WebSockets;
using System.Text;
using react_chat_app_backend.Controllers.WSControllers;
using react_chat_app_backend.Helpers;
using react_chat_app_backend.Models;
using react_chat_app_backend.Repositories.Interfaces;
using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Services;

public class WSMessageService : IWSMessageService
{
    private IWSMessageRepository _wsMessageRepository;
    private IWSManager _wsManager;
    private WSHelpers _wsHelpers;
    private IFriendShipRepository _friendShipRepository;
    private IUserRepository _userRepository;

    public WSMessageService(IWSMessageRepository wsMessageRepository, IFriendShipRepository friendShipRepository, IUserRepository userRepository, IWSManager wsManager)
    {
        _wsMessageRepository = wsMessageRepository;
        _wsManager = wsManager;
        _wsHelpers = new WSHelpers();
        _friendShipRepository = friendShipRepository;
        _userRepository = userRepository;
    }

    public async Task Handle(WebSocket webSocket, byte[] buffer)
    {
        var messageType = _wsHelpers.GetMessageType(buffer);
        switch (messageType)
        {
            case WSMessageType.chatMessage:
                await HandleChatMessage(webSocket, buffer);
                break;
                
            case WSMessageType.chatHistory:
                await FetchHistory(webSocket, buffer);
                break;
        }
    }

    public async Task FetchHistory(WebSocket webSocket, byte[] buffer)
    {
        try
        {
            var request = _wsHelpers.GetModelData<ChatMessageHistoryRequest>(buffer);
            var userId1 = request.userId1;
            var userId2 = request.userId2;
            var skip = request.skip;
            var take = request.take;

            List<ChatMessage> messages;
            
            if (skip == 0 && take == 0) {
                messages = await _wsMessageRepository.GetAllMessages(userId1, userId2);   
            } else {
                messages = await _wsMessageRepository.GetMessageSequence(userId1, userId2, skip, take);
            }
            
            var messageHistory = new { messages, type = "chatHistory" };
            buffer = _wsHelpers.ToJsonByteArray(messageHistory);
            await SendMessage(webSocket, buffer);
        }
        catch (Exception e)
        {
            buffer = _wsHelpers.ToJsonByteArray(new { message = "An unexpected error occurred on the server. Please try again later.", type = "error" });
            await SendMessage(webSocket, buffer);
        }
    }

    public async Task HandleChatMessage(WebSocket webSocket, byte[] buffer)
    {
        try
        {
            var message = _wsHelpers.GetModelData<ChatMessage>(buffer);
            var senderId = message.senderId;
            var receiverId = message.receiverId;
            var user = await _userRepository.GetUser(senderId);
            message.name = user.name;
            message.photoURL = user.photoURL;
        
            
            if (await CheckFriendshipExists(senderId, receiverId)
                && await CheckFriendshipPending(senderId, receiverId) == false)
            {
                await Store(message);
                await Forward(message);   
            }
        }
        catch (Exception e)
        {
            buffer = _wsHelpers.ToJsonByteArray(new { message = "An unexpected error occurred on the server. Please try again later.", type = "error" });
            await SendMessage(webSocket, buffer);
        }
    }
    
    public async Task Store(ChatMessage chatMessage)
    {
        await _wsMessageRepository.AddMessage(chatMessage);
    }
    
    public async Task Forward(ChatMessage chatMessage)
    {
        var receiverId = chatMessage.receiverId;
        var wsClient = _wsManager.Get(receiverId);
        
        if (wsClient == null)
            return;
        
        var wsMessage = new { chatMessage, type = "chatMessage" };
        var buffer = _wsHelpers.ToJsonByteArray(wsMessage);
        var bufferLength = _wsHelpers.GetLengthWithoutPadding(buffer);
        
        var receiverConnection = wsClient.webSocket;
        await SendMessage(receiverConnection, buffer, bufferLength);
    }
    
    public async Task SendToUser(string userId, string json)
    {
        var wsClient = _wsManager.Get(userId);

        if (wsClient == null)
            return;
        
        var userConnection = wsClient.webSocket;
        var buffer = Encoding.UTF8.GetBytes(json);
        await SendMessage(userConnection, buffer);
    }

    public async Task SendMessage(WebSocket webSocket, byte[] buffer, int bufferLength = 0)
    {
        if (webSocket.State != WebSocketState.Open) {
            return;
        }
        
        if (bufferLength == 0) {
            bufferLength = buffer.Length;
        }
        
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, bufferLength),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }

    // TODO: circular dependency for FriendShipService and WSMessageService
    public async Task<bool> CheckFriendshipExists(string userId1, string userId2)
    {
        var friendship = await _friendShipRepository.GetFriendShip(userId1, userId2);
        return friendship != null;
    }
    
    public async Task<bool> CheckFriendshipPending(string userId1, string userId2)
    {
        var friendship = await _friendShipRepository.GetFriendShip(userId1, userId2);
        return friendship != null && friendship.isPending;
    }
    
}