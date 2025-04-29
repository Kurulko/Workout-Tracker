using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

public class ExerciseSetDTO
{
    public long Id { get; set; }

    public ModelWeight? TotalWeight { get; set; }
    public TimeSpanModel? TotalTime { get; set; }
    public int? TotalReps { get; set; }

    public ModelWeight? Weight { get; set; }
    public TimeSpanModel? Time { get; set; }

    [PositiveNumber]
    public int? Reps { get; set; } = null;

    public long ExerciseId { get; set; }
    public string ExerciseName { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; }
    public string? ExercisePhoto { get; set; }
}
