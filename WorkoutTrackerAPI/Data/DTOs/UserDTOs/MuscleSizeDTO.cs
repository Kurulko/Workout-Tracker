using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class MuscleSizeDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }

    [PositiveNumber]
    public float Size { get; set; }
    public SizeType SizeType { get; set; }

    public long MuscleId { get; set; }
    public MuscleDTO? Muscle { get; set; }
}
