using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Domain.Entities.Exercises;

public class ExerciseAlias : BaseWorkoutModel
{
    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; } = null!;
}
