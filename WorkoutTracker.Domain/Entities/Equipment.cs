using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Users;

namespace WorkoutTracker.Domain.Entities;

public class Equipment : BaseWorkoutModel
{
    public string? Image { get; set; }

    public string? OwnedByUserId { get; set; }
    public IEnumerable<Exercise>? Exercises { get; set; }
}
