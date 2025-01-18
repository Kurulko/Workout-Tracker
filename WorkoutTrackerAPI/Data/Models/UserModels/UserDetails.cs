namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class UserDetails : IDbModel
{
    public long Id { get; set; }
    public Gender? Gender { get; set; }
    public ModelSize? Height { get; set; }
    public ModelWeight? Weight { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public double? BodyFatPercentage { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }
}

public enum Gender
{
    Male, Female
}
