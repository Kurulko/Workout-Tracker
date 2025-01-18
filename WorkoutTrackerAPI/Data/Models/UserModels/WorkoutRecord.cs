using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.Models.WorkoutModels;

public class WorkoutRecord : IDbModel
{
    public long Id { get; set; }
    public TimeSpan Time { get; set; }
    public DateTime Date { get; set; }

    public long WorkoutId { get; set; }
    public Workout? Workout { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }

    public IEnumerable<ExerciseRecordGroup> ExerciseRecordGroups { get; set; } = null!;
}
