using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;

namespace WorkoutTracker.Domain.Entities.Workouts;

public class WorkoutRecord : IDbModel
{
    public long Id { get; set; }
    public TimeSpan Time { get; set; }
    public DateTime Date { get; set; }

    public string UserId { get; set; } = null!;

    public long WorkoutId { get; set; }
    public Workout? Workout { get; set; }

    public IEnumerable<ExerciseRecordGroup> ExerciseRecordGroups { get; set; } = null!;
}
