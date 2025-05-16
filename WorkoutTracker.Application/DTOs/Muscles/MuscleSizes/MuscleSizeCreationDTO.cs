using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;

public class MuscleSizeCreationDTO
{
    public DateTime Date { get; set; }
    public ModelSize Size { get; set; }

    public long MuscleId { get; set; }
}
