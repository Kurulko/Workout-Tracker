namespace WorkoutTrackerAPI.Data.Models.ProgressModels;

public class TotalCompletedProgress
{
    public ModelWeight TotalWeightLifted { get; set; }
    public int TotalRepsCompleted { get; set; }
    public TimeSpan TotalTimeCompleted { get; set; }
}
