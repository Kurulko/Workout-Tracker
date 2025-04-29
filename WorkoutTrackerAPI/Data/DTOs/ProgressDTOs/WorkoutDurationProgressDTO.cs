namespace WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;

public class WorkoutDurationProgressDTO
{
    public TimeSpanModel AverageWorkoutDuration { get; set; }
    public TimeSpanModel MinWorkoutDuration { get; set; }
    public TimeSpanModel MaxWorkoutDuration { get; set; }
}
