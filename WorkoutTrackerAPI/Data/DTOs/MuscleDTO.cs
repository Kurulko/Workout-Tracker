using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class MuscleDTO : WorkoutModel
{
    public byte[]? Image { get; set; }

    public long? ParentMuscleId { get; set; }
    public Muscle? ParentMuscle { get; set; } = null!;
    public IEnumerable<Muscle>? ChildMuscles { get; set; } = null!;
}
