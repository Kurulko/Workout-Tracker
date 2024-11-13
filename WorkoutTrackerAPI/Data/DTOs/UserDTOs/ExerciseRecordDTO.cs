using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class ExerciseRecordDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }

    [PositiveNumber]
    public int CountOfTimes { get; set; }

    [PositiveNumber]
    public double? Weight { get; set; }
    public TimeSpan? Time { get; set; }

    [PositiveNumber]
    public int? Reps { get; set; }

    [PositiveNumber]
    public double? SumOfWeight { get; set; }
    public TimeSpan? SumOfTime { get; set; }

    [PositiveNumber]
    public int? SumOfReps { get; set; }

    public long ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
}
