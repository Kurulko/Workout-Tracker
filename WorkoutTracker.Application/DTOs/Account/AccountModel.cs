using System.Text.Json.Serialization;

namespace WorkoutTracker.Application.DTOs.Account;

public abstract class AccountModel
{
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;

    [JsonPropertyName("rememberme")]
    public bool RememberMe { get; set; }
}