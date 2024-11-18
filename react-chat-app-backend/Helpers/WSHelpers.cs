using System.Text;
using System.Text.Json;
using react_chat_app_backend.Controllers.WSControllers;
using react_chat_app_backend.Models;

namespace react_chat_app_backend.Helpers;

public class WSHelpers
{
    public byte[] ObjectToJsonByteArray(object obj)
    {
        var json = JsonSerializer.Serialize(obj);  
        return Encoding.UTF8.GetBytes(json);
    }
    
    public T ByteArrayToObject<T>(byte[] buffer)
    {
        var json = Encoding.UTF8.GetString(buffer).Trim('\0');
        return JsonSerializer.Deserialize<T>(json);
    }

    public int GetLengthWithoutPadding(byte[] buffer)
    {
        var i = buffer.Length - 1;
        while (i >= 0 && buffer[i] == 0) i--;
        return i + 1;
    }
    
    public WSMessageType GetMessageType(byte[] buffer)
    {
        var typeStr = ByteArrayToObject<ModelType>(buffer).type;
        return Enum.Parse<WSMessageType>(typeStr);
    }
}