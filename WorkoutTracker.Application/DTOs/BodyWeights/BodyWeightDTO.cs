using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.BodyWeights;

public class BodyWeightDTO
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public ModelWeight Weight { get; set; }
}
