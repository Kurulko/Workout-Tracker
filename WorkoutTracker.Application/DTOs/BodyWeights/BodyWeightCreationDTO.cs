using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.BodyWeights;

public class BodyWeightCreationDTO
{
    public DateTime Date { get; set; }
    public ModelWeight Weight { get; set; }
}
