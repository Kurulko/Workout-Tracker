using WorkoutTrackerAPI.Data.Models.ProgressModels;

namespace WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;

public class BaseInfoProgressDTO
{
    public int TotalWorkouts { get; set; }
    public TimeSpanModel TotalDuration { get; set; }
    public int CountOfExercisesUsed { get; set; }

    public double FrequencyPerWeek { get; set; }
    public IEnumerable<DateTime>? WorkoutDates { get; set; }
}
