namespace WorkoutTrackerAPI.Data.Models.WorkoutModels;

public class ExerciseAlias : WorkoutModel
{
    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; } = null!;
}
