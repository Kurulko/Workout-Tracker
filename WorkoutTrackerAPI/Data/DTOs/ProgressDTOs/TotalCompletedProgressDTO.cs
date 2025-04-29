namespace WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;

public class TotalCompletedProgressDTO
{
    public ModelWeight TotalWeightLifted { get; set; }
    public int TotalRepsCompleted { get; set; }
    public TimeSpanModel TotalTimeCompleted { get; set; }
}
