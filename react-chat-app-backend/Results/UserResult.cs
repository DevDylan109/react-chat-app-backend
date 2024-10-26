using react_chat_app_backend.Results.Enums;

namespace react_chat_app_backend.Results;

public class UserResult
{
    public UserOutcome UserOutcome { get; }
    public string Message { get; }

    public UserResult (UserOutcome userOutcome, string message)
    {
        UserOutcome = userOutcome;
        Message = message;
    }
    
    public static UserResult UserCreated() =>
        new UserResult(UserOutcome.UserCreated, "The user has been created.");
    
    public static UserResult UserDeleted() =>
        new UserResult(UserOutcome.UserDeleted, "The user has been deleted.");
    
    public static UserResult UserNotFound() =>
        new UserResult(UserOutcome.UserNotFound, "The user has not been found.");
    
    public static UserResult ChangedUsername() =>
        new UserResult(UserOutcome.ChangedUsername, "You have successfully changed your username.");
    
    public static UserResult ChangedProfilePicture() =>
        new UserResult(UserOutcome.ChangedProfilePicture, "You have successfully changed your profile picture.");
    
    public static UserResult UserAlreadyExists() =>
        new UserResult(UserOutcome.UserAlreadyExists, "This user already exists.");
    
    public static UserResult InputInvalid() =>
        new UserResult(UserOutcome.InputInvalid, "The received input is invalid.");

}