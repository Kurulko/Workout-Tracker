namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs;

public class TotalUserProgressDTO
{
    public int TotalWorkouts { get; set; }
    public ModelWeight TotalWeightLifted { get; set; }
    public TimeSpanModel TotalDuration { get; set; }
    public DateTime? FirstWorkoutDate { get; set; }
    public int CountOfDaysSinceFirstWorkout { get; set; }
    public TimeSpanModel AverageWorkoutDuration { get; set; }
    public double FrequencyPerWeek { get; set; }
    public IEnumerable<DateTime>? WorkoutDates { get; set; }
}
