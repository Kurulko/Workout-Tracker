using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.Models;

public class Muscle : WorkoutModel
{
    public byte[]? Image { get; set; }

    public Muscle? ParentMuscle { get; set; } = null!;
    public IEnumerable<Muscle>? ChildMuscles { get; set; } = null!;
    public IEnumerable<Exercise>? Exercises { get; set; } = null!;
    public IEnumerable<MuscleSize>? MuscleSizes { get; set; } = null!;
}
