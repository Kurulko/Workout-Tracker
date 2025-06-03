using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Domain.Entities.Muscles;

public class MuscleAlias : BaseWorkoutModel
{
    public long MuscleId { get; set; }
    public Muscle? Muscle { get; set; } = null!;
}
