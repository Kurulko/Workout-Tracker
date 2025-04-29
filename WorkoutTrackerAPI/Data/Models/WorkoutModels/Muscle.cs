using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.Models;

public class Muscle : WorkoutModel
{
    public string? Image { get; set; }
    public bool IsMeasurable { get; set; }

    public long? ParentMuscleId { get; set; }
    public Muscle? ParentMuscle { get; set; }
    public ICollection<Muscle>? ChildMuscles { get; set; }
    public IEnumerable<Exercise>? Exercises { get; set; }
    public IEnumerable<MuscleSize>? MuscleSizes { get; set; }
}
