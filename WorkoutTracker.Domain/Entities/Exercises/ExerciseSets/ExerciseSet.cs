using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;

public class ExerciseSet : IDbModel
{
    public long Id { get; set; }

    public ModelWeight? Weight { get; set; }
    public TimeSpan? Time { get; set; }
    public int? Reps { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public long ExerciseSetGroupId { get; set; }
    public ExerciseSetGroup? ExerciseSetGroup { get; set; }
}
