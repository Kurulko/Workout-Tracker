using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.Models.Progresses;

public class TotalCompletedProgress
{
    public ModelWeight TotalWeightLifted { get; set; }
    public int TotalRepsCompleted { get; set; }
    public TimeSpan TotalTimeCompleted { get; set; }
}
