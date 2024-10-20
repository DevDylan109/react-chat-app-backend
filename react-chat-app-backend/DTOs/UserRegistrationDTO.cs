using System.ComponentModel.DataAnnotations;

namespace react_chat_app_backend.DTOs;


public class UserRegistrationDTO
{ 
    [Required(ErrorMessage = "Display Name is required.")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "Display Name must be between 6 and 30 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Display Name should not contain special characters.")]
    public string DisplayName { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 30 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Username should not contain special characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string Password { get; set; }
}
