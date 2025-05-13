using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Domain.Entities;

public class Equipment : BaseWorkoutModel
{
    public string? Image { get; set; }

    public string? OwnedByUserId { get; set; }
    public IEnumerable<Exercise>? Exercises { get; set; }
}
