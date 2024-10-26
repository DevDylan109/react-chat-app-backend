using react_chat_app_backend.Results.Enums;

namespace react_chat_app_backend.Results;

public class FriendShipResult
{
    public FriendShipOutcome Outcome { get; }
    public string Message { get; }

    public FriendShipResult(FriendShipOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public static FriendShipResult FriendRequestAccepted () =>
        new FriendShipResult(FriendShipOutcome.FriendShipAccepted, "This friend request has been accepted.");
    
    public static FriendShipResult FriendRequestDeclined () =>
        new FriendShipResult(FriendShipOutcome.FriendShipDeclined, "This friend request has been accepted.");
    
    public static FriendShipResult FriendShipCreated (string receiverId) =>
        new FriendShipResult(FriendShipOutcome.FriendShipCreated, $"Friend request to: {receiverId} has been sent.");

    public static FriendShipResult FriendShipDeleted () =>
        new FriendShipResult(FriendShipOutcome.FriendShipDeleted, $"The friendship has been deleted.");
    
    public static FriendShipResult FriendShipIsPending() =>
        new FriendShipResult(FriendShipOutcome.FriendShipIsPending, "You already have a pending friend request with this user.");

    public static FriendShipResult FriendShipAlreadyExists() =>
        new FriendShipResult(FriendShipOutcome.FriendShipAlreadyExists, "The friendship does already exist.");
    
    public static FriendShipResult FriendShipDoesNotExist() =>
        new FriendShipResult(FriendShipOutcome.FriendShipDoesNotExist, "The friendship does not exist.");
    
    public static FriendShipResult FriendShipAlreadyAccepted() =>
        new FriendShipResult(FriendShipOutcome.FriendShipAlreadyAccepted, "You are already friends with this user.");
    
    public static FriendShipResult UserNotFound() =>
        new FriendShipResult(FriendShipOutcome.UserNotFound, "The user that you want to add does not exist.");
    
    public static FriendShipResult UserAddSelf() =>
        new FriendShipResult(FriendShipOutcome.UserAddSelf, "You can not befriend yourself.");
    
}