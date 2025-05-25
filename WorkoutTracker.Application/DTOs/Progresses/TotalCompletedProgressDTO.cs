using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Progresses;

public class TotalCompletedProgressDTO
{
    public ModelWeight TotalWeightLifted { get; set; }
    public int TotalRepsCompleted { get; set; }
    public TimeSpanModel TotalTimeCompleted { get; set; }
}
