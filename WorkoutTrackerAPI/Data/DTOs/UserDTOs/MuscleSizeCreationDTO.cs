using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs;

public class MuscleSizeCreationDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }

    [PositiveNumber]
    public float Size { get; set; }
    public SizeType SizeType { get; set; }

    public long MuscleId { get; set; }
}
