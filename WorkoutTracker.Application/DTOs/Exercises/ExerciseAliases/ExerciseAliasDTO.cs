namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseAliases;

public class ExerciseAliasDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long ExerciseId { get; set; }
}
