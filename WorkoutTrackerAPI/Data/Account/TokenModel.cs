namespace WorkoutTrackerAPI.Data.Account;

public class TokenModel
{
    public string TokenStr { get; set; } = null!;
    public int ExpirationDays { get; set; }
    public string[] Roles { get; set; } = null!;
}