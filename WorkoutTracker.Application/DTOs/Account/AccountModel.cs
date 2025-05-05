using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WorkoutTracker.Domain.Entities.Users;

namespace WorkoutTracker.Application.DTOs.Account;

public abstract class AccountModel
{
    [Required(ErrorMessage = "Enter your name!")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Enter your password!")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "{0} must be at least {1} characters long")]
    public string Password { get; set; } = null!;

    [Display(Name = "Remember me?")]
    [JsonPropertyName("rememberme")]
    public bool RememberMe { get; set; }
}