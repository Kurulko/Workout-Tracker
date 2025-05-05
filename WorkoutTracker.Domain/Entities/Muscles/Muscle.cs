using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Domain.Entities.Muscles;

public class Muscle : BaseWorkoutModel
{
    public string? Image { get; set; }
    public bool IsMeasurable { get; set; }

    public long? ParentMuscleId { get; set; }
    public Muscle? ParentMuscle { get; set; }
    public ICollection<Muscle>? ChildMuscles { get; set; }
    public IEnumerable<Exercise>? Exercises { get; set; }
    public IEnumerable<MuscleSize>? MuscleSizes { get; set; }
}
