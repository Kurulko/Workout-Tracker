using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class MuscleSizeDTO : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public float Size { get; set; }
    public SizeType SizeType { get; set; }

    public long MuscleId { get; set; }
    public Muscle? Muscle { get; set; }
}
