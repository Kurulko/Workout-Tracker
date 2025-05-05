using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;

public class MuscleSizeDTO
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public ModelSize Size { get; set; }

    public long MuscleId { get; set; }
    public string? MuscleName { get; set; }
    public string? MusclePhoto { get; set; }
}
