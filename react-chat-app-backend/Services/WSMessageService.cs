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
    private IFriendShipRepository _friendShipRepository;
    private IUserRepository _userRepository;
    private ITokenService _tokenService;
    private WSHelpers _wsHelpers;
    
    public WSMessageService(
        IWSMessageRepository wsMessageRepository,
        IFriendShipRepository friendShipRepository,
        IUserRepository userRepository,
        IWSManager wsManager,
        ITokenService tokenService)
    {
        _wsMessageRepository = wsMessageRepository;
        _wsManager = wsManager;
        _friendShipRepository = friendShipRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _wsHelpers = new WSHelpers();
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
        var request = _wsHelpers.GetModelData<ChatMessageHistoryRequest>(buffer);
        var userId1 = request.userId1;
        var userId2 = request.userId2;
        var skip = request.skip;
        var take = request.take;
        List<ChatMessage> messages;
        
        try
        {
            // if (_tokenService.IsTokenValid(userId1, token)){}
            
            if (skip == 0 && take == 0) {
                messages = await _wsMessageRepository.GetAllMessages(userId1, userId2);   
            } else {
                messages = await _wsMessageRepository.GetMessageSequence(userId1, userId2, skip, take);
            }
            
            var messageHistory = new { messages, type = "chatHistory" };
            buffer = _wsHelpers.ToJsonByteArray(messageHistory);
            await SendResponseMessage(webSocket, buffer);
        }
        catch (Exception e)
        {
            buffer = _wsHelpers.ToJsonByteArray(new { message = "An unexpected error occurred on the server. Please try again later.", error = e, type = "error" });
            await SendResponseMessage(webSocket, buffer);
        }
    }

    public async Task HandleChatMessage(WebSocket webSocket, byte[] buffer)
    {
        var message = _wsHelpers.GetModelData<ChatMessage>(buffer);
        var senderId = message.senderId;
        var receiverId = message.receiverId;
        var token = message.token;
        message.token = ""; // recipient user can steal the token
        var user = await _userRepository.GetUser(senderId);
        message.name = user.name;
        message.photoURL = user.photoURL;
        
        try
        {
            if (await CheckFriendshipExists(senderId, receiverId)
                && await CheckFriendshipPending(senderId, receiverId) == false
                 && _tokenService.IsTokenValid(senderId, token))
            {
                await Store(message);
                await Forward(message);   
            }
        }
        catch (Exception e)
        {
            buffer = _wsHelpers.ToJsonByteArray(new { message = "An unexpected error occurred on the server. Please try again later.", type = "error" });
            await SendResponseMessage(webSocket, buffer);
        }
    }
    
    public async Task Store(ChatMessage chatMessage)
    {
        await _wsMessageRepository.AddMessage(chatMessage);
    }
    
    public async Task Forward(ChatMessage chatMessage)
    {
        var wsMessage = new { chatMessage, type = "chatMessage" };
        var buffer = _wsHelpers.ToJsonByteArray(wsMessage);
        var bufferLength = _wsHelpers.GetLengthWithoutPadding(buffer);
        await SendMessage(chatMessage.receiverId, buffer, bufferLength);
    }
    
    public async Task SendToUser(string userId, string json)
    {
        // var wsClient = _wsManager.Get(userId);
        //
        // if (wsClient == null)
        //     return;
        // var userConnection = wsClient.webSocket;
        var buffer = Encoding.UTF8.GetBytes(json);
        await SendMessage(userId, buffer);
    }

    public async Task SendMessage(string userId, byte[] buffer, int bufferLength = 0)
    {
        if (bufferLength == 0) {
            bufferLength = buffer.Length;
        }

        // A user can log into multiple devices at the same time
        var connections = _wsManager.All()
            .Where(ws => ws.userId == userId)
            .Select(ws => ws.webSocket)
            .ToArray();
        
        foreach (var conn in connections)
        {
            if (conn.State != WebSocketState.Open) {
                continue;
            }
            
            await conn.SendAsync(
                new ArraySegment<byte>(buffer, 0, bufferLength),
                WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None
            );
        }
    }
    
    public async Task SendResponseMessage(WebSocket webSocket, byte[] buffer, int bufferLength = 0)
    {
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

    public async Task BroadcastMessage(string userId, object obj)
    { 
        var wsClients = _wsManager.All();
        var payload = _wsHelpers.ToJsonByteArray(obj);

        foreach (var wsClient in wsClients) {
            await SendMessage(wsClient.userId, payload);
        }
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