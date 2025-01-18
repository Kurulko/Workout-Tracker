using System.Collections.Generic;

namespace WorkoutTrackerAPI.Data;

public class TotalUserProgress
{
    public int TotalWorkouts { get; set; }
    public ModelWeight TotalWeightLifted { get; set; }
    public TimeSpan TotalDuration { get; set; } 
    public DateTime? FirstWorkoutDate { get; set; }

    public int CountOfDaysSinceFirstWorkout { get; set; }
    public TimeSpan AverageWorkoutDuration { get; set; }
    public double FrequencyPerWeek { get; set; }
    public IEnumerable<DateTime>? WorkoutDates { get; set; }
}
