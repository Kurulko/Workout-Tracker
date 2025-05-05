using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.ValidationAttributes;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;

public class MuscleSizeCreationDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }

    public ModelSize Size { get; set; }
    public long MuscleId { get; set; }
}
