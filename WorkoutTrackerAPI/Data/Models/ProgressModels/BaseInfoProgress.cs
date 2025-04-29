namespace WorkoutTrackerAPI.Data.Models.ProgressModels;

public class BaseInfoProgress
{
    public int TotalWorkouts { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public int CountOfExercisesUsed { get; set; }

    public int CountOfWorkoutDays { get; set; }
    public double FrequencyPerWeek { get; set; }
    public IEnumerable<DateTime>? WorkoutDates { get; set; }
}

