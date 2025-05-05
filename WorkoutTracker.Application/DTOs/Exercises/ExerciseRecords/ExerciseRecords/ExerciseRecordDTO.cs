using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
public class ExerciseRecordDTO
{
    public long Id { get; set; }
    public DateTime Date { get; set; }

    public ModelWeight? TotalWeight { get; set; }
    public TimeSpanModel? TotalTime { get; set; }
    public int? TotalReps { get; set; }

    public ModelWeight? Weight { get; set; }
    public TimeSpanModel? Time { get; set; }
    public int? Reps { get; set; }

    public long ExerciseId { get; set; }
    public string ExerciseName { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; }
    public string? ExercisePhoto { get; set; }

    public long WorkoutId { get; set; }
    public string WorkoutName { get; set; } = null!;
}
