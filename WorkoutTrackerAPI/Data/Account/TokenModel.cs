namespace WorkoutTrackerAPI.Data.Account;

public class TokenModel
{
    public string Token { get; set; } = null!;
    public int ExpirationDays { get; set; }
    public string[] Roles { get; set; } = null!;
}