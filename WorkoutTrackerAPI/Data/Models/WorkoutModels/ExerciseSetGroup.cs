using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

namespace WorkoutTrackerAPI.Data.Models.WorkoutModels;

public class ExerciseSetGroup : IDbModel
{
    public long Id { get; set; }

    public long WorkoutId { get; set; }
    public Workout? Workout { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public IEnumerable<ExerciseSet> ExerciseSets { get; set; } = null!;
}
