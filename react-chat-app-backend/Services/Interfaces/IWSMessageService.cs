using System.Net.WebSockets;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IWSMessageService
{
    Task Handle(WebSocket webSocket, byte[] buffer);
    Task FetchHistory(WebSocket webSocket, byte[] buffer);
    Task StoreAndForward(byte[] buffer);
    Task Store(WSMessage wsMessage);
    Task Forward(WSMessage wsMessage);
    Task SendToUser(string userId, string json);
}