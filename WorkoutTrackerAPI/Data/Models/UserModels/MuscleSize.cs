namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class MuscleSize : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public ModelSize Size { get; set; }

    public long MuscleId { get; set; }
    public Muscle? Muscle { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }
}

