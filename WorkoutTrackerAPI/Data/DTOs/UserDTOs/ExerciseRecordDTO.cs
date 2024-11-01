using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class ExerciseRecordDTO
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int CountOfTimes { get; set; }

    public double? Weight { get; set; }
    public TimeSpan? Time { get; set; }
    public int? Reps { get; set; }

    public double? SumOfWeight { get; set; }
    public TimeSpan? SumOfTime { get; set; }
    public int? SumOfReps { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
}
