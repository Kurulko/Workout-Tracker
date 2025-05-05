using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.Models.Progresses;

public class WorkoutWeightLiftedProgress
{
    public ModelWeight AverageWorkoutWeightLifted { get; set; }
    public ModelWeight MinWorkoutWeightLifted { get; set; }
    public ModelWeight MaxWorkoutWeightLifted { get; set; }
}
