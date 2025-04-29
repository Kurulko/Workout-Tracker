namespace WorkoutTrackerAPI.Data.Models.ProgressModels;

public class WorkoutDurationProgress
{
    public TimeSpan AverageWorkoutDuration { get; set; }
    public TimeSpan MinWorkoutDuration { get; set; }
    public TimeSpan MaxWorkoutDuration { get; set; }
}
