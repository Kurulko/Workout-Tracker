namespace WorkoutTracker.Application.Models.Progresses;

public class WorkoutDurationProgress
{
    public TimeSpan AverageWorkoutDuration { get; set; }
    public TimeSpan MinWorkoutDuration { get; set; }
    public TimeSpan MaxWorkoutDuration { get; set; }
}
