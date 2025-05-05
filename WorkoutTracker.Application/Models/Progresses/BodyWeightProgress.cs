using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.Models.Progresses;

public class BodyWeightProgress
{
    public ModelWeight AverageBodyWeight { get; set; }
    public ModelWeight MinBodyWeight { get; set; }
    public ModelWeight MaxBodyWeight { get; set; }
}
