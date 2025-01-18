namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class BodyWeight : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public ModelWeight Weight { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }
}

