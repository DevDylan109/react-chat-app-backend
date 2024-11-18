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

    public async Task HandleIncomingMessage(WebSocket webSocket, byte[] buffer)
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
        var request = _wsHelpers.ByteArrayToObject<ChatMessageHistoryRequest>(buffer);
        List<ChatMessage> messages;
        
        try
        {
            if (request.skip == 0 && request.take == 0) {
                messages = await _wsMessageRepository.GetAllMessages(request.userId1, request.userId2);   
            } else {
                messages = await _wsMessageRepository.GetMessageSequence(request.userId1, request.userId2, request.skip, request.take);
            }
            
            await SendResponse(webSocket, new { messages, type = "chatHistory" });
        }
        catch (Exception e)
        {
            await SendResponse(webSocket, new { e.Message, type = "error" });
        }
    }

    public async Task HandleChatMessage(WebSocket webSocket, byte[] buffer)
    {
        var chatMessage = _wsHelpers.ByteArrayToObject<ChatMessage>(buffer);
        chatMessage.token = ""; // recipient user can steal the token
        
        try
        {
            if (await CheckFriendshipExists(chatMessage.senderId, chatMessage.receiverId)
                && await CheckFriendshipPending(chatMessage.senderId, chatMessage.receiverId) == false
                 && _tokenService.IsTokenValid(chatMessage.senderId, chatMessage.token))
            {
                await _wsMessageRepository.StoreMessage(chatMessage);
                await SendMessage(chatMessage.receiverId, new { chatMessage, type = "chatMessage" });
            }
        }
        catch (Exception e)
        {
            await SendResponse(webSocket, new { e.Message, type = "error" });
        }
    }

    public async Task SendMessage(string userId, object obj)
    {
        var buffer = _wsHelpers.ObjectToJsonByteArray(obj);
        
        // Prevent trailing 0 bytes
        var bufferLength = _wsHelpers.GetLengthWithoutPadding(buffer);

        // A user can have more websocket connections
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
    
    public async Task SendResponse(WebSocket webSocket, object obj)
    {
        var buffer = _wsHelpers.ObjectToJsonByteArray(obj);
        var bufferLength = _wsHelpers.GetLengthWithoutPadding(buffer);

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
        var payload = _wsHelpers.ObjectToJsonByteArray(obj);

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