using react_chat_app_backend.Services.Interfaces;

namespace react_chat_app_backend.Services;

public class TokenService : ITokenService
{
    private List<Token> _tokenCache = new();
    
    public string CreateAndStore(string userId, int expirationInMinutes)
    {
        var newToken = new Token(userId, 120);
        _tokenCache.Add(newToken);
        return newToken.Value;
    }

    public string[] Retrieve(string userId)
    {
        var tokens = _tokenCache
            .Where(t => t.UserId == userId)
            .Select(t => t.Value)
            .ToArray();
        
        return tokens;
    }

    public void LazyClear()
    {
        _tokenCache.RemoveAll(t => t.ExpDate < DateTime.UtcNow);
    }

    public bool IsTokenValid (string userId, string token)
    {
        LazyClear();
        var retrievedTokens = Retrieve(userId);
        return retrievedTokens.Any(retrievedToken => retrievedToken == token);
    }
}

class Token
{
    public string Value { get; }
    public string UserId { get; }
    public DateTime ExpDate { get;}

    public Token(string userId, int expirationInMinutes)
    {
        UserId = userId;
        Value = Guid.NewGuid().ToString();
        ExpDate = DateTime.UtcNow.AddMinutes(expirationInMinutes);
    }
}