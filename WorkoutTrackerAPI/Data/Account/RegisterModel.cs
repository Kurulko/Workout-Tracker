using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.Account;

public class RegisterModel : AccountModel
{
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Display(Name = "Confirm password")]
    [Required(ErrorMessage = "Repeat password")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least {1} characters long")]
    [Compare("Password", ErrorMessage = "Passwords don't match")]
    [JsonPropertyName("passwordconfirm")]
    public string PasswordConfirm { get; set; } = null!;

    public static explicit operator User(RegisterModel register)
        => new() { Email = register.Email, UserName = register.Name};
}
