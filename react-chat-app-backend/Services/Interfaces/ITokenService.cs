namespace react_chat_app_backend.Services.Interfaces;

public interface ITokenService
{
    public string CreateAndStore(string userId, int expirationInMinutes);
    public string[] Retrieve(string userId);
    public void LazyClear();
    public bool IsTokenValid(string userId, string token);
}