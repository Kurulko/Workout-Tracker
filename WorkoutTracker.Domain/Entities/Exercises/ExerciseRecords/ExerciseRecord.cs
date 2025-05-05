using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;

public class ExerciseRecord : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }

    public ModelWeight? Weight { get; set; }
    public TimeSpan? Time { get; set; }
    public int? Reps { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public long ExerciseRecordGroupId { get; set; }
    public ExerciseRecordGroup? ExerciseRecordGroup { get; set; }

    public string UserId { get; set; } = null!;
}
