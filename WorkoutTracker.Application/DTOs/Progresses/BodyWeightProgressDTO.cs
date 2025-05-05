using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Progresses;

public class BodyWeightProgressDTO
{
    public ModelWeight AverageBodyWeight { get; set; }
    public ModelWeight MinBodyWeight { get; set; }
    public ModelWeight MaxBodyWeight { get; set; }
}
