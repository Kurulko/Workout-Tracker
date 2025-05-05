namespace WorkoutTracker.Domain.Base;

public abstract class BaseWorkoutModel : IDbModel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}