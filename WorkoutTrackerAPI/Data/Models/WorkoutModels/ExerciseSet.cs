using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

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

    public string UserId { get; set; } = null!;
    public User? User { get; set; }
}
