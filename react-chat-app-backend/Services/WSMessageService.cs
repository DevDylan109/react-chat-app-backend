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
    // FriendShipService _friendShipService;
    private IFriendShipRepository _friendShipRepository;

    public WSMessageService(IWSMessageRepository wsMessageRepository, IFriendShipRepository friendShipRepository, IWSManager wsManager)
    {
        _wsMessageRepository = wsMessageRepository;
        _wsManager = wsManager;
        _wsHelpers = new WSHelpers();
        //_friendShipService = friendShipService;
        _friendShipRepository = friendShipRepository;
    }

    public async Task Handle(WebSocket webSocket, byte[] buffer)
    {
        var messageType = _wsHelpers.GetMessageType(buffer);
        switch (messageType)
        {
            case WSMessageType.chatMessage:
                await StoreAndForward(buffer);
                break;
                
            case WSMessageType.chatHistory:
                await FetchHistory(webSocket, buffer);
                break;
        }
    }

    public async Task FetchHistory(WebSocket webSocket, byte[] buffer)
    {
        var userIds = _wsHelpers.GetModelData<Users>(buffer);
        var userId1 = userIds.userId1;
        var userId2 = userIds.userId2;

        var messages = await _wsMessageRepository.GetMessages(userId1, userId2);
        var messageHistory = new { messages, type = WSMessageType.chatHistory };
        
        buffer = _wsHelpers.ToJsonByteArray(messageHistory);

        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }
    
    public async Task StoreAndForward(byte[] buffer)
    {
        var message = _wsHelpers.GetModelData<WSMessage>(buffer);
        var senderId = message.senderId;
        var receiverId = message.receiverId;
        
        if (await CheckFriendshipExists(senderId, receiverId))
        {
            await Store(message);
            await Forward(message);   
        }
    }
    
    public async Task Store(WSMessage wsMessage)
    {
        await _wsMessageRepository.AddMessage(wsMessage);
    }
    
    public async Task Forward(WSMessage wsMessage)
    {
        var receiverId = wsMessage.receiverId;
        var wsClient = _wsManager.Get(receiverId);

        if (wsClient == null)
            return;
        
        var receiverConnection = wsClient.webSocket;
        var buffer = _wsHelpers.ToJsonByteArray(wsMessage);
        var bufferLength = _wsHelpers.GetLengthWithoutPadding(buffer);
        await receiverConnection.SendAsync(
            new ArraySegment<byte>(buffer, 0, bufferLength),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }
    
    public async Task SendToUser(string userId, string json)
    {
        var wsClient = _wsManager.Get(userId);

        if (wsClient == null)
            return;
        
        var userConnection = wsClient.webSocket;
        var buffer = Encoding.UTF8.GetBytes(json);
        await userConnection.SendAsync(
            new ArraySegment<byte>(buffer, 0, buffer.Length),
            WebSocketMessageType.Text,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None
        );
    }
    
    // TODO: Fix circular dependency for FriendShipService and WSMessageService :(
    public async Task<bool> CheckFriendshipExists(string userId1, string userId2)
    {
        var friendship = await _friendShipRepository.GetFriendShip(userId1, userId2);
        return friendship != null;
    }
    
}