using System.ComponentModel.DataAnnotations.Schema;

namespace react_chat_app_backend.Models;

public class FriendRequest
{
    public string initiatorId { get; set; }
    public string acceptorId { get; set; }

    public FriendRequest(string initiatorId, string acceptorId)
    {
        this.initiatorId = initiatorId;
        this.acceptorId = acceptorId;
    }

}