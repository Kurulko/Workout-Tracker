using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

public class ExerciseSetGroupDTO
{
    public long Id { get; set; }

    public ModelWeight Weight { get; set; }
    public int Sets { get; set; }

    public long ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
    public ExerciseType? ExerciseType { get; set; }

    public long WorkoutId { get; set; }
    public string? WorkoutName { get; set; }

    public IEnumerable<ExerciseSetDTO> ExerciseSets { get; set; } = null!;
}
