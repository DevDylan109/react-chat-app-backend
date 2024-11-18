using System.Net.WebSockets;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Services.Interfaces;

public interface IWSMessageService
{
    Task Handle(WebSocket webSocket, byte[] buffer);
    Task BroadcastMessage(string userId, object obj);
    Task SendMessage(string userId, object obj);
    Task FetchHistory(WebSocket webSocket, byte[] buffer);
    Task HandleChatMessage(WebSocket webSocket, byte[] buffer);
}