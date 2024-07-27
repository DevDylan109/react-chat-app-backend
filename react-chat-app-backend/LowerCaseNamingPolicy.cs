using System.Text.Json;

namespace react_chat_app_backend;

public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToLower();
}