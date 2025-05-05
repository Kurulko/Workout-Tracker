using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Progresses;

public class WorkoutWeightLiftedProgressDTO
{
    public ModelWeight AverageWorkoutWeightLifted { get; set; }
    public ModelWeight MinWorkoutWeightLifted { get; set; }
    public ModelWeight MaxWorkoutWeightLifted { get; set; }
}
